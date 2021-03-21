using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.Connections.WebSockets
{
    public sealed class UnhandledMessageEventArgs
    {
        public MessageBuffer Message { get; init; }
        public bool IsHandled { get; private set; }

        public void SetHandled()
        {
            IsHandled = true;
        }
    }

    public sealed class MessageEventArgs
    {
        public MessageBuffer Message { get; init; }
    }

    public abstract class AbstractWebSocketConnection : IDisposable
    {
        protected abstract ValueTask<ValueWebSocketReceiveResult> ReceiveBufferAsync(Memory<byte> buffer, CancellationToken cancellationToken);
        protected abstract ValueTask SendBufferAsync(ReadOnlyMemory<byte> buffer, bool isBinary, bool isEOM, CancellationToken cancellationToken);
        protected abstract Task DisconnectAsync();
        protected abstract bool IsConnected();

        private volatile bool _isRunning;
        private bool _disposed;

        private readonly List<IMessageHandler> _handlers = new();
        private readonly object _receiveLock = new();
        private readonly SemaphoreSlim _sendLock = new(1);
        private readonly byte[] _readBuffer = new byte[1024];
        private readonly ConcurrentBag<MessageBuffer> _reusedMessages = new();
        private CancellationTokenSource _connectionStop;

        private readonly AggregateEventManager<UnhandledMessageEventArgs> _unhandledMessageEvent = new();
        public event EventHandler<UnhandledMessageEventArgs> UnhandledMessage
        {
            add => _unhandledMessageEvent.Add(value);
            remove => _unhandledMessageEvent.Remove(value);
        }

        private readonly AggregateEventManager<MessageEventArgs> _messageSendingEvent = new();
        public event EventHandler<MessageEventArgs> MessageSending
        {
            add => _messageSendingEvent.Add(value);
            remove => _messageSendingEvent.Remove(value);
        }

        private readonly AggregateEventManager<MessageEventArgs> _messageReceivedEvent = new();
        public event EventHandler<MessageEventArgs> MessageReceived
        {
            add => _messageReceivedEvent.Add(value);
            remove => _messageReceivedEvent.Remove(value);
        }

        private readonly AggregateEventManager<EventArgs> _receiveLoopExitedEvent = new();
        public event EventHandler<EventArgs> ReceiveLoopExited
        {
            add => _receiveLoopExitedEvent.Add(value);
            remove => _receiveLoopExitedEvent.Remove(value);
        }

        protected bool IsRunning
        {
            get => _isRunning;
            set => _isRunning = value;
        }
        private Task _startTask;

        public int MaxReusedMessageBuffers { get; set; } = 10;

        //TODO subprotocols?

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CancelAllHandlers();
                    _sendLock.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void CancelAllHandlers()
        {
            lock (_receiveLock)
            {
                //Make a copy. In case some handlers do something bad.
                foreach (var h in _handlers.ToArray())
                {
                    h.Cancel();
                }
                _handlers.Clear();
            }
        }

        //This is only called from the main message loop and we don't need a lock.
        private async Task<MessageBuffer> ReceiveMessageInternalAsync()
        {
            var ret = CreateMessageBuffer();
            ValueWebSocketReceiveResult received = default;

            ret.Clear();

            //Try reading into MS's buffer (avoids a copy step).
            if (ret.Data.Capacity != 0)
            {
                var underlyingBuffer = ret.Data.GetBuffer();

                //If we first read and then SetLength, the extended part will be zeroed.
                //We need to first SetLength.
                var underlyingBufferLen = underlyingBuffer.Length - 1; //Make it safer?
                ret.Data.SetLength(underlyingBufferLen);
                underlyingBuffer = ret.Data.GetBuffer(); //In case it reallocates.
                var underlyingBufferMemory = new Memory<byte>(underlyingBuffer, 0, underlyingBufferLen);

                received = await ReceiveBufferAsync(underlyingBufferMemory, _connectionStop.Token);
                if (received.MessageType == WebSocketMessageType.Close)
                {
                    ReturnMessageBuffer(ret);
                    return null;
                }
                ret.Data.SetLength(received.Count);
                ret.Data.Position = received.Count;
            }

            //Not enough space. Use our buffer and extend.
            while (!received.EndOfMessage)
            {
                received = await ReceiveBufferAsync(_readBuffer, _connectionStop.Token);
                if (received.MessageType == WebSocketMessageType.Close)
                {
                    ReturnMessageBuffer(ret);
                    return null;
                }
                ret.Data.Write(_readBuffer, 0, received.Count);
                if (received.EndOfMessage)
                {
                    break;
                }
            }

            ret.IsBinary = received.MessageType == WebSocketMessageType.Binary;

            try
            {
                _messageReceivedEvent.Invoke(this, new() { Message = ret });
            }
            catch (Exception e)
            {
                //TODO log
            }
            return ret;
        }

        private async Task SendMessageInternalAsync(MessageBuffer msg)
        {
            if (!IsConnected())
            {
                throw new InvalidOperationException();
            }
            try
            {
                _messageSendingEvent.Invoke(this, new() { Message = msg });
            }
            catch (Exception e)
            {
                //TODO log
            }
            await _sendLock.WaitAsync();
            try
            {
                var buffer = msg.Data.GetBuffer();
                await SendBufferAsync(new ReadOnlyMemory<byte>(buffer, 0, (int)msg.Data.Length), msg.IsBinary, true, _connectionStop.Token);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        //Start receiver loop. This method is not thread safe.
        private async Task StartAsync()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The connection has already started.");
            }
            IsRunning = true;
            _connectionStop = new();
            List<IMessageHandler> handlerList = new();
            while (IsRunning && IsConnected())
            {
                try
                {
                    var msg = await ReceiveMessageInternalAsync();
                    if (msg is null)
                    {
                        //Close signal received. Disconnect.
                        await DisconnectAsync();
                        IsRunning = false;
                        //TODO log
                    }
                    else
                    {
                        HandleMessageInternal(msg, handlerList);
                    }
                }
                catch (Exception e)
                {
                    //TODO log
                }
            }
            try
            {
                _receiveLoopExitedEvent.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                //TODO log
            }
        }

        //TODO need to provide some information to help user decide whether to reconnect.
        public void Start()
        {
            _startTask = StartAsync();
        }

        public async Task StopAsync()
        {
            IsRunning = false;

            await DisconnectAsync();
            //_connectionStop.Cancel();

            var startTask = Interlocked.Exchange(ref _startTask, null);
            await startTask;
        }

        private void FindHandler(MessageBuffer message, List<IMessageHandler> results)
        {
            results.Clear();
            lock (_receiveLock)
            {
                for (int i = 0; i < _handlers.Count; ++i)
                {
                    var handler = _handlers[i];
                    try
                    {
                        if (handler.CanHandle(message))
                        {
                            results.Add(handler);
                            if (handler.HasNextFilter)
                            {
                                //Replace the receiver with a new one.
                                //This is necessary because a loop-based handler might finish processing the
                                //previous one when the next comes.
                                var nextHandler = handler.GetNextFilter();
                                nextHandler.Init(this);
                                _handlers[i] = nextHandler;
                            }
                            else
                            {
                                _handlers.RemoveAt(i);
                                i -= 1;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //TODO log
                    }
                }
            }
        }

        //This must be called inside receiver task (no concurrent call).
        private void HandleMessageInternal(MessageBuffer message, List<IMessageHandler> handlerList)
        {
            FindHandler(message, handlerList);

            if (handlerList.Count != 0)
            {
                //Trigger the continuation of the receiver's task.
                for (int i = 0; i < handlerList.Count; ++i)
                {
                    var copy = message;
                    if (i != handlerList.Count - 1)
                    {
                        copy = message.Owner.CreateMessageBuffer();
                        message.CopyTo(copy);
                    }
                    try
                    {
                        handlerList[i].SetResult(copy);
                    }
                    catch (Exception e)
                    {
                        //TODO log
                    }
                }
            }
            else
            {
                //Cannot find a proper filter.
                try
                {
                    _unhandledMessageEvent.Invoke(this, new() { Message = message });
                }
                catch (Exception e)
                {
                    //TODO log
                }
            }
        }

        public MessageBuffer CreateMessageBuffer()
        {
            if (_reusedMessages.TryTake(out var ret))
            {
                return ret;
            }
            return new(this);
        }

        internal void ReturnMessageBuffer(MessageBuffer messageBuffer)
        {
            messageBuffer.Clear();
            if (_reusedMessages.Count < MaxReusedMessageBuffers)
            {
                _reusedMessages.Add(messageBuffer);
            }
        }

        //Send a message without waiting for a reply.
        public Task SendMessageAsync(MessageBuffer msg)
        {
            return SendMessageInternalAsync(msg);
        }

        //Send a message and use the receiver to receive the reply.
        //The task finishes when the sending finishes.
        public Task SendMessageAsync(MessageBuffer msg, IMessageHandler receiver)
        {
            AddMessageHandler(receiver);
            return SendMessageAsync(msg);
        }

        public void AddMessageHandler(IMessageHandler messageHandler)
        {
            messageHandler.Init(this);
            lock (_receiveLock)
            {
                _handlers.Add(messageHandler);
            }
        }

        //TODO receive Func<MB, bool> filtered as async
        //TODO send and receive reply as async
        //TODO send and receive json data (as extension)

        public static AbstractWebSocketConnection Create(WebSocket webSocket)
        {
            return new WebSocketConnection(webSocket);
        }
        
        public static async Task<AbstractWebSocketConnection> CreateClient(string url)
        {
            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(url), CancellationToken.None);
            return Create(ws);
        }

        public static async Task<AbstractWebSocketConnection> CreateServer(HttpListenerContext httpListenerContext)
        {
            var wsContext = await httpListenerContext.AcceptWebSocketAsync(null);
            return Create(wsContext.WebSocket);
        }
    }
}
