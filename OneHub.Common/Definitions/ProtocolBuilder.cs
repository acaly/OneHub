using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public static partial class ProtocolBuilder
    {
        private static readonly Dictionary<Type, ProtocolInfo> _protocols = new();
        private static readonly Dictionary<Type, Func<AbstractWebSocketConnection, object>> _wsInterface = new();
        private static readonly Dictionary<Type, Func<object, IMessageHandler>> _wsImpl = new();

        private static readonly ModuleBuilder _dynamicModule;

        static ProtocolBuilder()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("OneHub.ProtocolBuilderAssembly"),
                    AssemblyBuilderAccess.Run);
            _dynamicModule = assembly.DefineDynamicModule(assembly.GetName().Name);
        }

        public static T BuildWebSocketInterface<T>(AbstractWebSocketConnection connection)
            where T : class
        {
            if (!_wsInterface.TryGetValue(typeof(T), out var factory))
            {
                factory = BuildWebSocketInterfaceFactory(typeof(T), GetProtocolInfo(typeof(T)));
                _wsInterface[typeof(T)] = factory;
            }
            return (T)factory(connection);
        }

        private static Func<AbstractWebSocketConnection, object> BuildWebSocketInterfaceFactory(Type t, ProtocolInfo protocolInfo)
        {
            var type = _dynamicModule.DefineType(t.FullName + "_WebSocketInterface", TypeAttributes.Public);
            type.AddInterfaceImplementation(t);

            var conn = type.DefineField("_connection", typeof(AbstractWebSocketConnection),
                FieldAttributes.Private | FieldAttributes.InitOnly);
            CodeGenFieldInitLists fieldInitLists = new();

            //Apis.
            foreach (var (requestType, responseType, requestDispatcher) in protocolInfo.Apis)
            {
                //TODO need to have a method for protocols to customize member names (and maybe signature?)
                var interfaceMethodName = requestType.Name + "Async";
                var interfaceMethod = t.GetMethod(interfaceMethodName);
                var parameters = interfaceMethod.GetParameters();
                if (interfaceMethod.ReturnType != typeof(Task<>).MakeGenericType(responseType) ||
                    parameters.Length != 1 || parameters[0].ParameterType != requestType)
                {
                    throw new ProtocolBuilderException($"API method {interfaceMethod} has wrong signature.");
                }

                var implMethod = type.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                    interfaceMethod.ReturnType, new[] { requestType });
                {
                    var il = implMethod.GetILGenerator();
                    var ilEnv = new ILGeneratorEnv(type, il, fieldInitLists);

                    //Prepare locals for dispatcher code.
                    var connLocal = il.DeclareLocal(typeof(AbstractWebSocketConnection));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, conn);
                    il.Emit(OpCodes.Stloc, connLocal);
                    var dataLocal = il.DeclareLocal(requestType);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stloc, dataLocal);

                    requestDispatcher.EmitSendAction(requestType, ilEnv, connLocal, dataLocal);
                    il.Emit(OpCodes.Ret); //EmitSendAction should leave a Task<TResponse> on stack.
                }
            }

            //Emit event handlers (OnMessageXXX).
            var singleIdCheck = RunSingleIdCheck(protocolInfo.Events);
            var eventHandlerMethods = EmitRecv(type, singleIdCheck, fieldInitLists, protocolInfo.Events, null);

            //Event entry (OnMessage).
            var eventEntryMethod = EmitRecvEntry(type, singleIdCheck, fieldInitLists,
                eventHandlerMethods, out var eventDispatchMethodListField);

            //Ctor.
            {
                var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis,
                    new[] { typeof(object[]), typeof(AbstractWebSocketConnection) });
                var il = ctor.GetILGenerator();
                var ilEnv = new ILGeneratorEnv(type, il, fieldInitLists);

                //We need a method to create the private type GeneratedMessageHandler from this OnMessage method.
                //Make a factory method and pass it into ctor through the value-init'ed field list.
                var msgFactoryField = ilEnv.AddReadOnlyField<Func<Action<MessageBuffer>, IMessageHandler>>("_sys_msgHandlerFactory",
                    func => new GeneratedMessageHandler(new() { OnMessageMethod = func }));

                //Init readonly fields.
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Stfld, conn);

                //Generate field init.
                fieldInitLists.Emit(il);
                //NO MORE AddFields after this.

                //Add handler (call OnMessage for messages from connection).

                il.Emit(OpCodes.Ldarg_2); //connection

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, msgFactoryField); //delegate
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, eventEntryMethod);
                il.Emit(OpCodes.Newobj, typeof(Action<MessageBuffer>)
                    .GetConstructor(new[] { typeof(object), typeof(IntPtr) })); //arg0
                il.Emit(OpCodes.Call, msgFactoryField.FieldType.GetMethod("Invoke")); //IMessageHandler

                il.Emit(OpCodes.Call, typeof(AbstractWebSocketConnection)
                    .GetMethod(nameof(AbstractWebSocketConnection.AddMessageHandler)));

                //Calculate eventDispatchMethodListField (single-id mode only).
                if (singleIdCheck.Enabled)
                {
                    EmitInitSingleIdDispatchMethodListField(il, eventDispatchMethodListField, eventHandlerMethods);
                }

                il.Emit(OpCodes.Ret);
            }

            var runtimeType = type.CreateType();
            var fieldData = fieldInitLists.GetData();
            return connection => Activator.CreateInstance(runtimeType, fieldData, connection);
        }

        public static IMessageHandler BuildWebSocketImpl<TInterface, TImpl>(TImpl obj)
            where TInterface : class
            where TImpl : class, TInterface
        {
            lock (_wsImpl)
            {
                if (!_wsImpl.TryGetValue(typeof(TImpl), out var p))
                {
                    p = BuildWebSocketImplFactory(typeof(TInterface), GetProtocolInfo(typeof(TInterface)));
                    _wsImpl[typeof(TImpl)] = p;
                }
                return p(obj);
            }
        }

        private static Func<object, IMessageHandler> BuildWebSocketImplFactory(Type protocolInterfaceType,
            ProtocolInfo protocolInfo)
        {
            var type = _dynamicModule.DefineType(protocolInterfaceType.FullName + "_WebSocketImplAdapter",
                TypeAttributes.Public);
            var fieldInitLists = new CodeGenFieldInitLists();

            var protocolField = type.DefineField("_sys_protocol", protocolInterfaceType,
                FieldAttributes.Private | FieldAttributes.InitOnly);
            var connectionField = type.DefineField("_sys_connection", typeof(AbstractWebSocketConnection),
                FieldAttributes.Private);

            //Emit api handlers (OnMessageXXX).
            var singleIdCheck = RunSingleIdCheck(protocolInfo.Apis);
            var apiHandlerMethods = EmitRecv(type, singleIdCheck, fieldInitLists, protocolInfo.Apis, protocolField);

            //Emit api handler entry (OnMessage).
            var onMessageMethod = EmitRecvEntry(type, singleIdCheck, fieldInitLists, apiHandlerMethods, out var apiDispatchMethodListField);

            //Emit event handlers (used to subscript impl's events).
            List<(MethodInfo handler, MethodInfo subscribe)> eventHandlers = new();
            foreach (var (eventType, _, eventDispatcher) in protocolInfo.Events)
            {
                var handler = type.DefineMethod("Handle" + eventType.Name, MethodAttributes.Private,
                    typeof(Task), new[] { typeof(object), eventType });
                var il = handler.GetILGenerator();
                var ilEnv = new ILGeneratorEnv(type, il, fieldInitLists);

                var connLocal = il.DeclareLocal(typeof(AbstractWebSocketConnection));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, connectionField);
                il.Emit(OpCodes.Stloc, connLocal);
                var eventDataLocal = il.DeclareLocal(eventType);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Stloc, eventDataLocal);
                eventDispatcher.EmitSendAction(eventType, ilEnv, connLocal, eventDataLocal);

                //Return the Task generated by dispatcher.
                il.Emit(OpCodes.Ret);

                //Allow custom event names?
                var subscribe = protocolInterfaceType.GetEvent(eventType.Name);
                if (subscribe.EventHandlerType != typeof(AsyncEventHandler<>).MakeGenericType(eventType))
                {
                    throw new ProtocolBuilderException($"Event {subscribe} does not match event type {eventType}.");
                }
                eventHandlers.Add((handler, subscribe.AddMethod));
            }

            var initMethod = type.DefineMethod("Init", MethodAttributes.Private,
                typeof(void), new[] { typeof(AbstractWebSocketConnection) });
            {
                var il = initMethod.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stfld, connectionField);
                il.Emit(OpCodes.Ret);
            }

            //Constructor.
            FieldBuilder factoryMethodField;
            {
                var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(object[]), typeof(object) });
                //Parameter is fieldInitValues and protocolObj (need a cast).

                var il = ctor.GetILGenerator();
                var ilEnv = new ILGeneratorEnv(type, il, fieldInitLists);

                //Pass a factory method into the class through field init mechanism.
                factoryMethodField = ilEnv.AddReadOnlyField<Func<Action<AbstractWebSocketConnection>, Action<MessageBuffer>, IMessageHandler>>("_sys_factory", (a1, a2) =>
                {
                    return new GeneratedMessageHandler(new GeneratedImplMethods
                    {
                        InitMethod = a1,
                        OnMessageMethod = a2,
                    });
                });

                //Save protocol obj in local.
                var protocolObjLocal = il.DeclareLocal(protocolInterfaceType);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Castclass, protocolInterfaceType);
                il.Emit(OpCodes.Stloc, protocolObjLocal);

                //Save protocol obj in field.
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc, protocolObjLocal);
                il.Emit(OpCodes.Stfld, protocolField);

                //Write init values for all other fields.
                fieldInitLists.Emit(il);
                //NO MORE AddFields after this.

                //Calculate apiDispatchMethodListField (single-id mode only).
                if (singleIdCheck.Enabled)
                {
                    EmitInitSingleIdDispatchMethodListField(il, apiDispatchMethodListField, apiHandlerMethods);
                }

                //Subscribe event handlers
                foreach (var (handler, subscribe) in eventHandlers)
                {
                    il.Emit(OpCodes.Ldloc, protocolObjLocal); //this
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldftn, handler);
                    il.Emit(OpCodes.Newobj, subscribe.GetParameters()[0].ParameterType
                        .GetConstructor(new[] { typeof(object), typeof(IntPtr) })); //handler (arg1)
                    il.Emit(OpCodes.Callvirt, subscribe);
                }

                il.Emit(OpCodes.Ret);
            }

            //Generate the factory method which created the IMessageHandler.
            {
                //This name is referenced below.
                var createMessageHandlerMethodName = "CreateMessageHandler";
                var method = type.DefineMethod(createMessageHandlerMethodName, MethodAttributes.Public | MethodAttributes.Static,
                    typeof(IMessageHandler), new[] { typeof(object) });
                var il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, factoryMethodField); //delegate

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, initMethod);
                il.Emit(OpCodes.Newobj, typeof(Action<AbstractWebSocketConnection>)
                    .GetConstructor(new[] { typeof(object), typeof(IntPtr) })); //a1 (arg1)

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, onMessageMethod);
                il.Emit(OpCodes.Newobj, typeof(Action<MessageBuffer>)
                    .GetConstructor(new[] { typeof(object), typeof(IntPtr) })); //a2 (arg2)

                il.Emit(OpCodes.Call, factoryMethodField.FieldType.GetMethod("Invoke")); //IMessageHandler
                il.Emit(OpCodes.Ret);

                var runtimeType = type.CreateType();
                var bindFactoryMethod = (Func<object, IMessageHandler>)Delegate.CreateDelegate(typeof(Func<object, IMessageHandler>),
                    runtimeType.GetMethod(createMessageHandlerMethodName));
                var fieldData = fieldInitLists.GetData();
                return protocol => bindFactoryMethod(Activator.CreateInstance(runtimeType, fieldData, protocol));
            }
        }

        //We don't want to expose any public interfaces to implement. This struct
        //wraps all methods of interest of the generated object.
        private struct GeneratedImplMethods
        {
            public Action<AbstractWebSocketConnection> InitMethod;
            public Action<MessageBuffer> OnMessageMethod;
        }

        private sealed class GeneratedMessageHandler : IMessageHandler
        {
            private readonly GeneratedImplMethods _generatedImplMethods;

            public GeneratedMessageHandler(GeneratedImplMethods generatedImplMethods)
            {
                _generatedImplMethods = generatedImplMethods;
            }

            public void Init(AbstractWebSocketConnection connection)
            {
                _generatedImplMethods.InitMethod?.Invoke(connection);
            }

            public bool CanHandle(MessageBuffer message)
            {
                return true;
            }

            public void SetResult(MessageBuffer message)
            {
                _generatedImplMethods.OnMessageMethod(message);
            }

            public void Cancel()
            {
            }

            public bool HasNextFilter => true;
            public IMessageHandler GetNextFilter() => this;
        }
    }
}
