using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions.Builder0
{
    internal static class InterfaceBuilder
    {
        private static readonly ModuleBuilder _dynamicModule;

        static InterfaceBuilder()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("OneHub.DynamicAssembly0"),
                    AssemblyBuilderAccess.Run);
            _dynamicModule = assembly.DefineDynamicModule(assembly.GetName().Name);
        }

        public static Func<AbstractWebSocketConnection, object> BuildWebSocketInterfaceFactory(Type t,
            ProtocolBuilder.ProtocolInfo protocolInfo)
        {
            var echoPrefix = t.Name;

            var type = _dynamicModule.DefineType(t.FullName + "_WebSocketInterface", TypeAttributes.Public);
            type.AddInterfaceImplementation(t);

            var conn = type.DefineField("_connection", typeof(AbstractWebSocketConnection),
                FieldAttributes.Private | FieldAttributes.InitOnly);
            //var jsonOptions = type.DefineField("_options", typeof(JsonSerializerOptions),
            //    FieldAttributes.Private | FieldAttributes.InitOnly);
            var echoIndex = type.DefineField("_echo", typeof(int), FieldAttributes.Private);

            //private string GetEcho()
            var getEchoMethod = type.DefineMethod("GetEcho", MethodAttributes.Private, typeof(string), Type.EmptyTypes);
            {
                var il = getEchoMethod.GetILGenerator();

                //echoIndex += 1;
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, echoIndex);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stfld, echoIndex);

                //return echoPrefix + echoIndex.ToString()
                il.Emit(OpCodes.Ldstr, echoPrefix);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, echoIndex);
                il.Emit(OpCodes.Box, typeof(int));
                il.Emit(OpCodes.Call, typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) }));
                il.Emit(OpCodes.Ret);
            }

            //Apis.
            foreach (var interfaceMethod in t.GetMethods())
            {
                if (!interfaceMethod.Name.EndsWith("Async") || interfaceMethod.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
                {
                    //We should allow other public methods.
                    //throw new ProtocolBuilderException($"API method {interfaceMethod} must be async.");
                    continue;
                }
                var interfaceMethodWithoutAsync = interfaceMethod.Name[..^5];
                var protocolMethod = protocolInfo.Apis.FirstOrDefault(api => api.name == interfaceMethodWithoutAsync);
                if (protocolMethod.name is null)
                {
                    //We should allow other public methods.
                    //throw new ProtocolBuilderException($"Cannot find API definition for method {interfaceMethod}.");
                    continue;
                }
                var retType = typeof(Task<>).MakeGenericType(protocolMethod.response);
                if (retType != interfaceMethod.ReturnType)
                {
                    throw new ProtocolBuilderException($"API method {interfaceMethod} has wrong return type (should be {retType}).");
                }
                var interfaceMethodParams = interfaceMethod.GetParameters();
                if (interfaceMethodParams.Length != 1 || interfaceMethodParams[0].ParameterType != protocolMethod.request)
                {
                    throw new ProtocolBuilderException($"API method {interfaceMethod} has wrong parameter type (should be {protocolMethod.request}).");
                }
                var helperMethod = typeof(EchoRequestHelper).GetMethod(nameof(EchoRequestHelper.SendRequestWithEchoAsync))
                    .MakeGenericMethod(protocolMethod.request, protocolMethod.response);

                //public Task<[retType]> [protocolMethod.name]([protocolMethod.request] p)
                var implMethod = type.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                    retType, new[] { protocolMethod.request });
                {
                    //return SendWithEchoAsync<Request, Response>(_conn, protocolMethod.name, GetEcho(), p, _options);
                    var il = implMethod.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, conn); //this._conn (arg0)

                    il.Emit(OpCodes.Ldstr, protocolMethod.name); //protocolMethod.name (arg1)

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, getEchoMethod); //this.GetEcho() (arg2)

                    il.Emit(OpCodes.Ldarg_1); //p (arg3)

                    il.Emit(OpCodes.Call, helperMethod);
                    il.Emit(OpCodes.Ret);
                }
            }

            //Events declaration.
            Dictionary<string, (FieldInfo manager, MethodInfo dispatcher)> eventMembers = new();
            {
                var toJsonDocumentMethod = typeof(MessageBuffer).GetMethod(nameof(MessageBuffer.ToJsonDocument));
                var getRootElementMethod = typeof(JsonDocument).GetProperty(nameof(JsonDocument.RootElement)).GetMethod;
                var tryGetPropertyMethod = typeof(JsonElement).GetMethod(nameof(JsonElement.TryGetProperty),
                    new[] { typeof(string), typeof(JsonElement).MakeByRefType() });
                var getStringMethod = typeof(JsonElement).GetMethod(nameof(JsonElement.GetString));
                var stringEqualsMethod = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string) });
                var deserializeMethod = typeof(MessageSerializer).GetMethod(nameof(MessageSerializer.Deserialize));

                foreach (var interfaceEvent in protocolInfo.Events)
                {
                    //Event
                    var eventManager = type.DefineField("_eventManager" + interfaceEvent.name,
                        typeof(AsyncEventManager<>).MakeGenericType(interfaceEvent.data),
                        FieldAttributes.Private | FieldAttributes.InitOnly);
                    {
                        var eventType = typeof(AsyncEventHandler<>).MakeGenericType(interfaceEvent.data);
                        var @event = type.DefineEvent(interfaceEvent.name, EventAttributes.None, eventType);

                        var addMethod = type.DefineMethod("add_" + interfaceEvent.name, MethodAttributes.Public | MethodAttributes.Virtual,
                            typeof(void), new[] { eventType });
                        {
                            var il = addMethod.GetILGenerator();
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, eventManager);
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Call, eventManager.FieldType.GetMethod(nameof(AsyncEventManager<object>.Add)));
                            il.Emit(OpCodes.Ret);
                        }
                        @event.SetAddOnMethod(addMethod);

                        var removeMethod = type.DefineMethod("remove_" + interfaceEvent.name, MethodAttributes.Public | MethodAttributes.Virtual,
                            typeof(void), new[] { eventType });
                        {
                            var il = removeMethod.GetILGenerator();
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld, eventManager);
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Call, eventManager.FieldType.GetMethod(nameof(AsyncEventManager<object>.Remove)));
                            il.Emit(OpCodes.Ret);
                        }
                        @event.SetRemoveOnMethod(removeMethod);
                    }

                    //Event dispatcher: bool OnMessageXXX(MessageBuffer).
                    {
                        var eventDispatcher = type.DefineMethod("OnMessage" + interfaceEvent.name, MethodAttributes.Private,
                            typeof(bool), new[] { typeof(MessageBuffer) });
                        var il = eventDispatcher.GetILGenerator();

                        var jsonDocument = il.DeclareLocal(typeof(JsonDocument));
                        var rootElement = il.DeclareLocal(typeof(JsonElement));
                        var idElement = il.DeclareLocal(typeof(JsonElement));
                        var retFalseLabel = il.DefineLabel();

                        //jsonDocument = msg.ToJsonDocument();
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Call, toJsonDocumentMethod);
                        il.Emit(OpCodes.Stloc, jsonDocument);
                        
                        //if (jsonDocument is null) return false;
                        il.Emit(OpCodes.Ldloc, jsonDocument);
                        il.Emit(OpCodes.Brfalse, retFalseLabel);
                        
                        //rootElement = jsonDocument.RootElement;
                        il.Emit(OpCodes.Ldloc, jsonDocument);
                        il.Emit(OpCodes.Call, getRootElementMethod);
                        il.Emit(OpCodes.Stloc, rootElement);

                        foreach (var (k, v) in interfaceEvent.ids)
                        {
                            //if (!rootElement.TryGetProperty(k, out idElement)) return false;
                            il.Emit(OpCodes.Ldloca, rootElement);
                            il.Emit(OpCodes.Ldstr, k);
                            il.Emit(OpCodes.Ldloca, idElement);
                            il.Emit(OpCodes.Call, tryGetPropertyMethod);
                            il.Emit(OpCodes.Brfalse, retFalseLabel);
                        
                            //if (!string.Equals(idElement.GetString(), v)) return false;
                            il.Emit(OpCodes.Ldloca, idElement);
                            il.Emit(OpCodes.Call, getStringMethod);
                            il.Emit(OpCodes.Ldstr, JsonOptions.ConvertString(v));
                            il.Emit(OpCodes.Call, stringEqualsMethod);
                            il.Emit(OpCodes.Brfalse, retFalseLabel);
                        }

                        //_ = _eventManager.InvokeAsync(this, msg.ReadJson<TEvent>());
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, eventManager); //_eventManager
                        il.Emit(OpCodes.Ldarg_0); //sender
                        {
                            il.Emit(OpCodes.Ldarg_1); //msg
                            il.Emit(OpCodes.Call, deserializeMethod.MakeGenericMethod(interfaceEvent.data)); //event data
                        }
                        il.Emit(OpCodes.Call, eventManager.FieldType.GetMethod(nameof(AsyncEventManager<object>.InvokeAsync)));
                        //Pop the Task (we don't want for its completion).
                        il.Emit(OpCodes.Pop);

                        //return true;
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Ret);

                        il.MarkLabel(retFalseLabel);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Ret);

                        eventMembers.Add(interfaceEvent.name, (eventManager, eventDispatcher));
                    }
                }
            }

            //OnMessage method.
            var onMessageMethod = type.DefineMethod("OnMessage", MethodAttributes.Public,
                typeof(void), new[] { typeof(MessageBuffer) });
            {
                var il = onMessageMethod.GetILGenerator();
                var retLabel = il.DefineLabel();

                foreach (var interfaceEvent in protocolInfo.Events)
                {
                    //Stop when finding the first event type.
                    //if (OnMessageXXX(msg)) return;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, eventMembers[interfaceEvent.name].dispatcher);
                    il.Emit(OpCodes.Brtrue, retLabel);
                }

                il.MarkLabel(retLabel);
                il.Emit(OpCodes.Ret);
            }

            var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis,
                new[] { typeof(AbstractWebSocketConnection) });
            {
                var il = ctor.GetILGenerator();

                //Init readonly fields.
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, conn);
                foreach (var (_, members) in eventMembers)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Newobj, members.manager.FieldType.GetConstructor(Type.EmptyTypes));
                    il.Emit(OpCodes.Stfld, members.manager);
                }

                //Add handler.
                il.Emit(OpCodes.Ldarg_1); //connection (arg0)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, onMessageMethod);
                il.Emit(OpCodes.Newobj, typeof(Action<MessageBuffer>).GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                il.Emit(OpCodes.Newobj, typeof(InterfaceServerMessageHandler).GetConstructor(new[] { typeof(Action<MessageBuffer>) })); //handler (arg1)
                il.Emit(OpCodes.Call, typeof(AbstractWebSocketConnection).GetMethod(nameof(AbstractWebSocketConnection.AddMessageHandler)));

                il.Emit(OpCodes.Ret);
            }

            var runtimeType = type.CreateType();
            return connection => Activator.CreateInstance(runtimeType, connection);
        }
    }
}
