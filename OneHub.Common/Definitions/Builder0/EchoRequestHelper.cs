﻿using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions.Builder0
{
    //This is only used by the dynamically generated module.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EchoRequestHelper
    {
        [MessageSerializer(typeof(OneXMessageSerializer<>))]
        private class ActualRequest<T> where T : class
        {
            [JsonConverter(typeof(JsonOptions.StringPropertyConverter))]
            public string Action { get; init; }
            public string Echo { get; init; }
            public T Params { get; init; }
        }

        [MessageSerializer(typeof(OneXMessageSerializer<>))]
        private class BinaryMixedActualRequest<T> : ActualRequest<T>, IBinaryMixedObject where T : class
        {
            MemoryStream IBinaryMixedObject.Stream
            {
                get => ((IBinaryMixedObject)Params).Stream;
                set => ((IBinaryMixedObject)Params).Stream = value;
            }
        }

        [MessageSerializer(typeof(OneXMessageSerializer<>))]
        private class ActualResponse<T> where T : class
        {
            public string Status { get; init; }
            public int Retcode { get; init; }
            public string Echo { get; init; }
            public T Data { get; init; }
        }

        [MessageSerializer(typeof(OneXMessageSerializer<>))]
        private class BinaryMixedActualResponse<T> : ActualResponse<T>, IBinaryMixedObject where T : class
        {
            MemoryStream IBinaryMixedObject.Stream
            {
                get => ((IBinaryMixedObject)Data).Stream;
                set => ((IBinaryMixedObject)Data).Stream = value;
            }
        }

        internal static Func<T, MessageBuffer, Task> MakeResponseHandler<T>(MethodInfo method, Type request, Type response)
            where T : class
        {
            var funcType = typeof(Func<,,>).MakeGenericType(typeof(T), request, typeof(Task<>).MakeGenericType(response));
            var func = Delegate.CreateDelegate(funcType, method, throwOnBindFailure: false);
            if (func is null)
            {
                throw new ProtocolBuilderException($"Signature of method {method} does not match protocol declaration.");
            }
            var makerG = typeof(EchoRequestHelper).GetMethod(nameof(MakeHandlerFunc), BindingFlags.NonPublic | BindingFlags.Static);
            var maker = makerG.MakeGenericMethod(typeof(T), request, response);
            var ret = maker.Invoke(null, new object[] { func });
            return (Func<T, MessageBuffer, Task>)ret;
        }

        private static Func<TThis, MessageBuffer, Task> MakeHandlerFunc<TThis, TRequest, TResponse>(
            Func<TThis, TRequest, Task<TResponse>> func)
            where TThis : class
            where TRequest : class
            where TResponse : class
        {
            return async (obj, msgBuffer) =>
            {
                string echo = null;
                TResponse response;
                try
                {
                    TRequest request;
                    if (IBinaryMixedObject.Helper<TRequest>.IsBinaryMixed)
                    {
                        var actualRequest = MessageSerializer.Deserialize<BinaryMixedActualRequest<TRequest>>(msgBuffer);
                        request = actualRequest.Params;
                        echo = actualRequest.Echo;
                    }
                    else
                    {
                        var actualRequest = MessageSerializer.Deserialize<ActualRequest<TRequest>>(msgBuffer);
                        request = actualRequest.Params;
                        echo = actualRequest.Echo;
                    }
                    response = await func(obj, request);
                }
                catch (Exception e)
                {
                    //TODO log

                    //Reuse the msgBuffer.
                    MessageSerializer.Serialize(msgBuffer, new ActualResponse<TResponse>()
                    {
                        Data = default,
                        Retcode = 1,
                        Status = "failed",
                        Echo = echo,
                    });
                    await msgBuffer.Owner.SendMessageAsync(msgBuffer);
                    return;
                }

                //Reuse the msgBuffer.
                if (IBinaryMixedObject.Helper<TResponse>.IsBinaryMixed)
                {
                    MessageSerializer.Serialize(msgBuffer, new BinaryMixedActualResponse<TResponse>()
                    {
                        Data = response,
                        Retcode = 0,
                        Echo = echo,
                        Status = "ok",
                    });
                }
                else
                {
                    MessageSerializer.Serialize(msgBuffer, new ActualResponse<TResponse>()
                    {
                        Data = response,
                        Retcode = 0,
                        Echo = echo,
                        Status = "ok",
                    });
                }
                await msgBuffer.Owner.SendMessageAsync(msgBuffer);
            };
        }

        internal static Action<ImplBuilder.V11ProtocolImplMessageHandler<T>, T> MakeEventSubscription<T, TEvent>(EventInfo eventInfo)
            where T : class
        {
            var subscribeFunc = (Action<T, AsyncEventHandler<TEvent>>)Delegate.CreateDelegate(typeof(Action<T, AsyncEventHandler<TEvent>>),
                eventInfo.AddMethod, throwOnBindFailure: false);
            if (subscribeFunc is null)
            {
                throw new ProtocolBuilderException($"Signature of event {eventInfo} does not match protocol declaration.");
            }
            return (msgHandler, protocol) =>
            {
                //Define the event handler.
                AsyncEventHandler<TEvent> eventHandler = (object sender, TEvent e) =>
                {
                    //Send to msgHandler
                    var connection = msgHandler.Connection;
                    var msg = connection.CreateMessageBuffer();
                    MessageSerializer.Serialize(msg, e);
                    return connection.SendMessageAsync(msg);
                };

                //Subscribe the event
                subscribeFunc(protocol, eventHandler);
            };
        }

        //This is used by the generated methods, so has to be public.
        public static async Task<TResponse> SendRequestWithEchoAsync<TRequest, TResponse>(AbstractWebSocketConnection conn,
            string action, string echo, TRequest p)
            where TRequest : class
            where TResponse : class
        {
            bool Filter(MessageBuffer msg)
            {
                if (msg.IsBinary) return false;
                var jsonDocument = msg.ToJsonDocument();
                if (jsonDocument is null || !jsonDocument.RootElement.TryGetProperty("echo", out var echoElement))
                {
                    return false;
                }
                return echoElement.GetString() == echo;
            }
            var taskSource = new TaskCompletionSource<TResponse>();

            //1. Handle response.
            void HandleResponse<T>() where T : ActualResponse<TResponse>
            {
                conn.AddMessageHandler(new ReplyMessageHandler<T>(Filter, async replyTask =>
                {
                    try
                    {
                        var reply = await replyTask;
                        if (reply.Status == "ok")
                        {
                            taskSource.SetResult(reply.Data);
                        }
                        else
                        {
                            taskSource.SetException(new ApiException { Api = action, Code = reply.Retcode });
                        }
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                }));
            }
            if (typeof(IBinaryMixedObject).IsAssignableFrom(typeof(TResponse)))
            {
                HandleResponse<BinaryMixedActualResponse<TResponse>>();
            }
            else
            {
                HandleResponse<ActualResponse<TResponse>>();
            }

            //2. Send request.
            Task SendRequestAsync<T>() where T : ActualRequest<TRequest>
            {
                var msg = conn.CreateMessageBuffer();
                MessageSerializer.Serialize(msg, new ActualRequest<TRequest>()
                {
                    Action = action,
                    Params = p,
                    Echo = echo,
                });
                return conn.SendMessageAsync(msg);
            }
            if (typeof(IBinaryMixedObject).IsAssignableFrom(typeof(TRequest)))
            {
                await SendRequestAsync<BinaryMixedActualRequest<TRequest>>();
            }
            else
            {
                await SendRequestAsync<ActualRequest<TRequest>>();
            }

            return await taskSource.Task;
        }
    }
}
