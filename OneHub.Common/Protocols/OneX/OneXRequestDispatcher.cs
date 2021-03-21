using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using OneHub.Common.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    internal sealed class OneXRequestDispatcher : IMessageDispatcher
    {
        internal class Helper<T>
        {
            public static bool IsBinaryMixed { get; } = typeof(IBinaryMixedObject).IsAssignableFrom(typeof(T));
        }

        public bool TryGetSingleDispatchId(Type type, out (string key, string value) val)
        {
            val = ("action", JsonOptions.ConvertString(type.Name));
            return true;
        }

        public int EmitRecvMultiIdCheck(Type type, ILGeneratorEnv il, Label retLabel, LocalBuilder jsonDocument)
        {
            throw new ProtocolBuilderException("OneX requests only support single-id dispatching.");
        }

        public void EmitRecvAction(Type type, ILGeneratorEnv il, LocalBuilder protocol, LocalBuilder msgBuffer)
        {
            var protocolType = type.GetCustomAttributes().OfType<IProtocolApiRequestAttribute>().Single().ProtocolType;
            var convName = JsonOptions.ConvertString(type.Name);
            var responseType = type.GetNestedType("Response");
            if (responseType is null)
            {
                throw new ProtocolBuilderException($"Request {responseType.FullName} does not declare a reply type.");
            }
            var methodName = type.Name + "Async";
            var method = protocolType.GetMethod(methodName, new[] { type });
            if (method.ReturnType != typeof(Task<>).MakeGenericType(responseType))
            {
                throw new ProtocolBuilderException($"Return type of API method {method} does not match response type {responseType.FullName}.");
            }

            //Prepare for an actionObj (stored in field) that does the hard work for us.
            //The type of the actionObj is Func<TProtocol, MessageBuffer, Task>.
            var bindApiType = typeof(Func<,,>).MakeGenericType(protocolType, type, typeof(Task<>).MakeGenericType(responseType));
            //Note that Delegate.CreateDelegate automatically handles virtual method overloading.
            var bindApiMethod = Delegate.CreateDelegate(bindApiType, method);
            var helperMethod = typeof(OneXRequestDispatcher)
                .GetMethod(nameof(HandleRecv), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(protocolType, type, responseType);
            var actionObj = helperMethod.Invoke(null, new[] { bindApiMethod });
            var actionField = il.AddField($"_onex_recvAction_{convName}", actionObj.GetType(), actionObj, isReadOnly: true);

            il.ILGenerator.Emit(OpCodes.Ldarg_0);
            il.ILGenerator.Emit(OpCodes.Ldfld, actionField); //delegate
            il.ILGenerator.Emit(OpCodes.Ldloc, protocol); //arg1
            il.ILGenerator.Emit(OpCodes.Ldloc, msgBuffer); //arg2
            il.ILGenerator.Emit(OpCodes.Call, helperMethod.ReturnType.GetMethod("Invoke"));
            //Leave the Task on the stack as required by IMessageDispatcher.EmitRecvAction.
        }

        private static Func<TProtocol, MessageBuffer, Task> HandleRecv<TProtocol, TRequest, TResponse>(
            Func<TProtocol, TRequest, Task<TResponse>> apiMethod)
            where TRequest : class
            where TResponse : class
        {
            return async (protocol, msgBuffer) =>
            {
                //TODO handle deserialize exception?
                ActualRequest<TRequest> actualRequest;
                if (Helper<TRequest>.IsBinaryMixed)
                {
                    actualRequest = MessageSerializer.Deserialize<BinaryMixedActualRequest<TRequest>>(msgBuffer);
                }
                else
                {
                    actualRequest = MessageSerializer.Deserialize<ActualRequest<TRequest>>(msgBuffer);
                }
                var request = actualRequest.Params;
                var echo = actualRequest.Echo;
                try
                {
                    var response = await apiMethod(protocol, request);
                    if (Helper<TResponse>.IsBinaryMixed)
                    {
                        MessageSerializer.Serialize(msgBuffer, new BinaryMixedActualResponse<TResponse>(echo, response));
                    }
                    else
                    {
                        MessageSerializer.Serialize(msgBuffer, new ActualResponse<TResponse>(echo, response));
                    }
                }
                catch (Exception e)
                {
                    MessageSerializer.Serialize(msgBuffer, new ActualResponse<TResponse>(echo, e));
                }
                await msgBuffer.Owner.SendMessageAsync(msgBuffer);
            };
        }

        public void EmitSendAction(Type type, ILGeneratorEnv il, LocalBuilder connection, LocalBuilder data)
        {
            var protocolType = type.GetCustomAttributes().OfType<IProtocolApiRequestAttribute>().Single().ProtocolType;
            var convName = JsonOptions.ConvertString(type.Name);
            var responseType = type.GetNestedType("Response");
            if (responseType is null)
            {
                throw new ProtocolBuilderException($"Request {responseType.FullName} does not declare a reply type.");
            }

            //To simplify the ILGeneratorEnv, we keep an instance of MakeEcho() for each request type.
            var echoField = il.AddReadOnlyField($"_onex_apiEcho_{convName}", MakeEcho(convName + "_"));
            var helper = typeof(OneXRequestDispatcher)
                .GetMethod(nameof(HandleSend), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(type, responseType);
            var helperBindType = typeof(Func<,,,,>).MakeGenericType(typeof(string), typeof(string), typeof(AbstractWebSocketConnection), type,
                typeof(Task<>).MakeGenericType(responseType));
            var helperBind = Delegate.CreateDelegate(helperBindType, helper);
            var helperField = il.AddField($"_onex_sendAction_{convName}", helperBindType, helperBind, isReadOnly: true);

            il.ILGenerator.Emit(OpCodes.Ldarg_0);
            il.ILGenerator.Emit(OpCodes.Ldfld, helperField); //delegate
            il.ILGenerator.Emit(OpCodes.Ldstr, type.Name); //apiName before conversion (will be converted by JsonSerializer) (arg1)
            il.ILGenerator.Emit(OpCodes.Ldarg_0);
            il.ILGenerator.Emit(OpCodes.Ldfld, echoField);
            il.ILGenerator.Emit(OpCodes.Call, typeof(Func<string>).GetMethod("Invoke")); //echo (arg2)
            il.ILGenerator.Emit(OpCodes.Ldloc, connection); //connection (arg3)
            il.ILGenerator.Emit(OpCodes.Ldloc, data); //data (arg4)
            il.ILGenerator.Emit(OpCodes.Call, helperBindType.GetMethod("Invoke"));
            //Leave the Task on the stack as required by IMessageDispatcher.EmitSendAction.
        }

        private static async Task<TResponse> HandleSend<TRequest, TResponse>(string apiName, string echo,
            AbstractWebSocketConnection connection, TRequest data)
            where TRequest : class
            where TResponse : class
        {
            var taskSource = new TaskCompletionSource<TResponse>();

            bool Filter(MessageBuffer msg)
            {
                var jsonDocument = msg.ToJsonDocument();
                if (jsonDocument is null || !jsonDocument.RootElement.TryGetProperty("echo", out var echoElement))
                {
                    return false;
                }
                return echoElement.GetString() == echo;
            }

            //1. Handle response.
            void HandleResponse<T>() where T : ActualResponse<TResponse>
            {
                connection.AddMessageHandler(new ReplyMessageHandler<T>(Filter, async replyTask =>
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
                            taskSource.SetException(new ApiException { Api = apiName, Code = reply.Retcode });
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
            var msgBuffer = connection.CreateMessageBuffer();
            try
            {
                if (typeof(IBinaryMixedObject).IsAssignableFrom(typeof(TRequest)))
                {
                    MessageSerializer.Serialize(msgBuffer, new BinaryMixedActualRequest<TRequest>()
                    {
                        Action = apiName,
                        Params = data,
                        Echo = echo,
                    });
                }
                else
                {
                    MessageSerializer.Serialize(msgBuffer, new ActualRequest<TRequest>()
                    {
                        Action = apiName,
                        Params = data,
                        Echo = echo,
                    });
                }
            }
            catch
            {
                msgBuffer.Dispose();
                throw;
            }
            await connection.SendMessageAsync(msgBuffer);

            return await taskSource.Task;
        }

        private static Func<string> MakeEcho(string apiName)
        {
            int val = 0;
            return () => apiName + (++val);
        }

        private sealed class ReplyMessageHandler<T> : AbstractMessageHandler<T> where T : class
        {
            private readonly Func<MessageBuffer, bool> _canHandle;

            public ReplyMessageHandler(Func<MessageBuffer, bool> canHandle, Func<ValueTask<T>, ValueTask> task)
                : base(task)
            {
                _canHandle = canHandle;
            }

            public override bool CanHandle(MessageBuffer message)
            {
                return _canHandle(message);
            }
        }

        [MessageSerializer(typeof(JsonOptions.TextMessageSerializer<>))]
        private class ActualRequest<T> where T : class
        {
            [JsonConverter(typeof(JsonOptions.ReadOnlyStringPropertyConverter))]
            public string Action { get; init; }
            public string Echo { get; init; }
            public T Params { get; init; }
        }

        [MessageSerializer(typeof(JsonOptions.BinaryMessageSerializer<>))]
        private class BinaryMixedActualRequest<T> : ActualRequest<T>, IBinaryMixedObject where T : class
        {
            MemoryStream IBinaryMixedObject.Stream
            {
                get => ((IBinaryMixedObject)Params).Stream;
                set => ((IBinaryMixedObject)Params).Stream = value;
            }
        }

        [MessageSerializer(typeof(JsonOptions.TextMessageSerializer<>))]
        private class ActualResponse<T> where T : class
        {
            public string Status { get; init; }
            public int Retcode { get; init; }
            public string Echo { get; init; }
            public T Data { get; init; }

            //For deserialization.
            public ActualResponse() { }

            public ActualResponse(string echo, T data)
            {
                Status = "ok";
                Retcode = 0;
                Echo = echo;
                Data = data;
            }

            public ActualResponse(string echo, Exception e)
            {
                Status = "failed";
                Retcode = -1;
                Echo = echo;
                Data = null;

                if (e is ApiException apiException)
                {
                    Retcode = apiException.Code;
                }
            }
        }

        [MessageSerializer(typeof(JsonOptions.BinaryMessageSerializer<>))]
        private class BinaryMixedActualResponse<T> : ActualResponse<T>, IBinaryMixedObject where T : class
        {
            MemoryStream IBinaryMixedObject.Stream
            {
                get => ((IBinaryMixedObject)Data).Stream;
                set => ((IBinaryMixedObject)Data).Stream = value;
            }

            //For deserialization.
            public BinaryMixedActualResponse() { }

            public BinaryMixedActualResponse(string echo, T data) : base(echo, data)
            {
            }

            public BinaryMixedActualResponse(string echo, Exception e) : base(echo, e)
            {
            }
        }
    }
}
