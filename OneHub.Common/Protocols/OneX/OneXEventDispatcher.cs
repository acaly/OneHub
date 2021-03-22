using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using OneHub.Common.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    internal class OneXEventDispatcher : IMessageDispatcher
    {
        public bool TryGetSingleDispatchId(Type type, out (string key, string value) val)
        {
            //OneX protocols will not use single-id dispatching. No need to try.
            val = default;
            return false;
        }

        public int EmitRecvMultiIdCheck(Type type, ILGeneratorEnv il, Label retLabel, LocalBuilder jsonDocument)
        {
            if (typeof(IBinaryMixedObject).IsAssignableFrom(type))
            {
                throw new ProtocolBuilderException($"Event {type} cannot be IBinaryMixedObject.");
            }

            var getRootElementMethod = typeof(JsonDocument).GetProperty(nameof(JsonDocument.RootElement)).GetMethod;
            var tryGetPropertyMethod = typeof(JsonElement).GetMethod(nameof(JsonElement.TryGetProperty),
                new[] { typeof(string), typeof(JsonElement).MakeByRefType() });
            var getStringMethod = typeof(JsonElement).GetMethod(nameof(JsonElement.GetString));
            var stringEqualsMethod = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string) });

            var rootElement = il.ILGenerator.DeclareLocal(typeof(JsonElement));
            var idElement = il.ILGenerator.DeclareLocal(typeof(JsonElement));

            //rootElement = jsonDocument.RootElement;
            il.ILGenerator.Emit(OpCodes.Ldloc, jsonDocument);
            il.ILGenerator.Emit(OpCodes.Call, getRootElementMethod);
            il.ILGenerator.Emit(OpCodes.Stloc, rootElement);

            var instance = Activator.CreateInstance(type); //Used to read the instance properties.
            int dispatchKeyCount = 0;
            foreach (var property in type.GetProperties())
            {
                if (!property.IsDefined(typeof(EventIdAttribute)))
                {
                    continue;
                }
                if (property.PropertyType != typeof(string))
                {
                    throw new ProtocolBuilderException($"Invalid event id {property}. Must be of string type.");
                }
                if (property.GetMethod is null || property.GetMethod.IsStatic)
                {
                    throw new ProtocolBuilderException($"Invalid event id {property}. Must be readable instance property.");
                }
                dispatchKeyCount += 1;
                var k = JsonOptions.ConvertString(property.Name);
                var v = (string)property.GetValue(instance);

                //if (!rootElement.TryGetProperty(k, out idElement)) return false;
                il.ILGenerator.Emit(OpCodes.Ldloca, rootElement);
                il.ILGenerator.Emit(OpCodes.Ldstr, k);
                il.ILGenerator.Emit(OpCodes.Ldloca, idElement);
                il.ILGenerator.Emit(OpCodes.Call, tryGetPropertyMethod);
                il.ILGenerator.Emit(OpCodes.Brfalse, retLabel);

                //if (!string.Equals(idElement.GetString(), v)) return false;
                il.ILGenerator.Emit(OpCodes.Ldloca, idElement);
                il.ILGenerator.Emit(OpCodes.Call, getStringMethod);
                il.ILGenerator.Emit(OpCodes.Ldstr, JsonOptions.ConvertString(v));
                il.ILGenerator.Emit(OpCodes.Call, stringEqualsMethod);
                il.ILGenerator.Emit(OpCodes.Brfalse, retLabel);
            }
            if (dispatchKeyCount == 0)
            {
                throw new ProtocolBuilderException($"Event {type} does not have event id.");
            }
            return -dispatchKeyCount; //Event with more keys checked first.
        }

        public void EmitRecvAction(Type type, ILGeneratorEnv il, LocalBuilder protocol, LocalBuilder msgBuffer)
        {
            var protocolType = type.GetCustomAttributes().OfType<IProtocolEventAttribute>().Single().ProtocolType;
            var convName = JsonOptions.ConvertString(type.Name);

            var eventManagerType = typeof(AsyncEventManager<>).MakeGenericType(type);
            var eventManagerField = il.AddField($"_onex_eventManager_{convName}", eventManagerType,
                il => il.Emit(OpCodes.Newobj, eventManagerType.GetConstructor(Type.EmptyTypes)), isReadOnly: true);

            //There is an additional work for event's recv: to define the event and its accessors.
            //The ProtocolBuilder does not know the implementation here.
            //TODO we may need a unified method for protocols to customize member names
            var eventMemberName = type.Name;
            var eventHandlerType = typeof(AsyncEventHandler<>).MakeGenericType(type);
            var generatingEvent = il.TypeBuilder.DefineEvent(eventMemberName, EventAttributes.None, eventHandlerType);
            {
                var addAccessor = il.TypeBuilder.DefineMethod("add_" + eventMemberName,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    typeof(void), new[] { eventHandlerType });
                var mil = addAccessor.GetILGenerator();
                mil.Emit(OpCodes.Ldarg_0);
                mil.Emit(OpCodes.Ldfld, eventManagerField);
                mil.Emit(OpCodes.Ldarg_1);
                mil.Emit(OpCodes.Call, eventManagerField.FieldType.GetMethod(nameof(AsyncEventManager<object>.Add)));
                mil.Emit(OpCodes.Ret);
                generatingEvent.SetAddOnMethod(addAccessor);
            }
            {
                var removeAccessor = il.TypeBuilder.DefineMethod("remove_" + eventMemberName,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    typeof(void), new[] { eventHandlerType });
                var mil = removeAccessor.GetILGenerator();
                mil.Emit(OpCodes.Ldarg_0);
                mil.Emit(OpCodes.Ldfld, eventManagerField);
                mil.Emit(OpCodes.Ldarg_1);
                mil.Emit(OpCodes.Call, eventManagerField.FieldType.GetMethod(nameof(AsyncEventManager<object>.Remove)));
                mil.Emit(OpCodes.Ret);
                generatingEvent.SetRemoveOnMethod(removeAccessor);
            }

            var helperActionGen = typeof(OneXEventDispatcher)
                .GetMethod(nameof(HandleRecv), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(type);
            var helperActionType = helperActionGen.ReturnType; //Func<object, AsyncEventManager<TEvent>, MessageBuffer, Task>
            var helperAction = helperActionGen.Invoke(null, null);
            var helperActionField = il.AddField($"_onex_eventDispatch_{convName}", helperActionType, helperAction, isReadOnly: true);

            il.ILGenerator.Emit(OpCodes.Ldarg_0);
            il.ILGenerator.Emit(OpCodes.Ldfld, helperActionField); //delegate
            il.ILGenerator.Emit(OpCodes.Ldarg_0); //sender (arg1)
            il.ILGenerator.Emit(OpCodes.Ldarg_0);
            il.ILGenerator.Emit(OpCodes.Ldfld, eventManagerField); //eventManager (arg2)
            il.ILGenerator.Emit(OpCodes.Ldloc, msgBuffer); //msgBuffer (arg3)
            il.ILGenerator.Emit(OpCodes.Call, helperActionType.GetMethod("Invoke"));
            //Leave the Task on the stack as required by IMessageDispatcher.EmitRecvAction.
        }

        private static Func<object, AsyncEventManager<TEvent>, MessageBuffer, Task> HandleRecv<TEvent>()
            where TEvent : class
        {
            return async (sender, eventManager, msgBuffer) =>
            {
                TEvent eventData = null;
                try
                {
                    //TODO handle deserialize exception?
                    eventData = MessageSerializer.Deserialize<TEvent>(msgBuffer);
                }
                catch (Exception e)
                {
                    //TODO log
                }
                finally
                {
                    msgBuffer.Dispose();
                }
                if (eventData is not null)
                {
                    //TODO handle exceptions? (see comments on AsyncEventManager)
                    await eventManager.InvokeAsync(sender, eventData);
                }
            };
        }
        
        public void EmitSendAction(Type type, ILGeneratorEnv il, LocalBuilder connection, LocalBuilder data)
        {
            var protocolType = type.GetCustomAttributes().OfType<IProtocolEventAttribute>().Single().ProtocolType;
            var convName = JsonOptions.ConvertString(type.Name);

            var helperActionGen = typeof(OneXEventDispatcher)
                .GetMethod(nameof(HandleSend), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(type);
            var helperAction = helperActionGen.Invoke(null, null);
            var helperActionType = helperActionGen.ReturnType;
            var helperActionField = il.AddField($"_onex_eventSend_{convName}", helperActionType, helperAction, isReadOnly: true);

            il.ILGenerator.Emit(OpCodes.Ldarg_0);
            il.ILGenerator.Emit(OpCodes.Ldfld, helperActionField); //delegate
            il.ILGenerator.Emit(OpCodes.Ldloc, connection); //arg1
            il.ILGenerator.Emit(OpCodes.Ldloc, data); //arg2
            il.ILGenerator.Emit(OpCodes.Call, helperActionType.GetMethod("Invoke"));
            //Leave the Task on the stack as required by IMessageDispatcher.EmitSendAction.
        }

        private static Func<AbstractWebSocketConnection, TEvent, Task> HandleSend<TEvent>()
        {
            return (connection, eventData) =>
            {
                var msgBuffer = connection.CreateMessageBuffer();
                try
                {
                    MessageSerializer.Serialize(msgBuffer, eventData);
                }
                catch
                {
                    msgBuffer.Dispose();
                    throw;
                }
                return connection.SendMessageAsync(msgBuffer);
            };
        }
    }
}
