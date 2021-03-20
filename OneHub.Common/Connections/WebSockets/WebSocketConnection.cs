using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.Connections.WebSockets
{
    internal sealed class WebSocketConnection : AbstractWebSocketConnection
    {
        private readonly WebSocket _webSocket;

        public WebSocketConnection(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webSocket.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IsClosed()
        {
            return _webSocket.State != WebSocketState.Open;
        }

        protected override Task DisconnectAsync()
        {
            if (_webSocket.State == WebSocketState.CloseSent || _webSocket.State == WebSocketState.Closed)
            {
                return Task.CompletedTask;
            }
            return _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        protected override ValueTask<ValueWebSocketReceiveResult> ReceiveBufferAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            if (_webSocket.State == WebSocketState.CloseSent)
            {
                //Allow receiving the close response.
                return _webSocket.ReceiveAsync(buffer, cancellationToken);
            }
            if (IsClosed())
            {
                return ValueTask.FromException<ValueWebSocketReceiveResult>(new OperationCanceledException());
            }
            return _webSocket.ReceiveAsync(buffer, cancellationToken);
        }

        protected override ValueTask SendBufferAsync(ReadOnlyMemory<byte> buffer, bool isBinary, bool isEOM, CancellationToken cancellationToken)
        {
            if (IsClosed())
            {
                return ValueTask.FromException(new OperationCanceledException());
            }
            return _webSocket.SendAsync(buffer, isBinary ? WebSocketMessageType.Binary : WebSocketMessageType.Text, isEOM,
                cancellationToken);
        }

        protected override bool IsConnected()
        {
            return !IsClosed();
        }
    }
}
