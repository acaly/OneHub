using OneHub.Common.Connections;
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
    using OnMessageMethodList = List<(MethodInfo method, int priority, string val)>;

    public static partial class ProtocolBuilder
    {
        private class ProtocolInfo
        {
            public ImmutableArray<(Type request, Type response, IMessageDispatcher dispatcher)> Apis { get; init; }
            public ImmutableArray<(Type e, Type /* null */, IMessageDispatcher dispatcher)> Events { get; init; }
        }

        private static ProtocolInfo GetProtocolInfo(Type protocolType)
        {
            lock (_protocols)
            {
                if (!_protocols.TryGetValue(protocolType, out var ret))
                {
                    List<(Type, Type, IMessageDispatcher)> apis = new();
                    List<(Type, Type, IMessageDispatcher)> events = new();
                    Dictionary<Type, IMessageDispatcher> cachedDispatchers = new();

                    foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
                    {
                        var requestAttr = t.GetCustomAttributes().OfType<IProtocolApiRequestAttribute>().SingleOrDefault();
                        if (requestAttr?.ProtocolType == protocolType)
                        {
                            var responseType = t.GetNestedType("Response");
                            //TODO check response attribute
                            if (responseType is null)
                            {
                                throw new ProtocolBuilderException($"Api definition {t} does not have a response type.");
                            }
                            if (!cachedDispatchers.TryGetValue(requestAttr.DispatcherType, out var dispatcher))
                            {
                                dispatcher = (IMessageDispatcher)Activator.CreateInstance(requestAttr.DispatcherType);
                                cachedDispatchers.Add(requestAttr.DispatcherType, dispatcher);
                            }
                            apis.Add((t, responseType, dispatcher));
                        }
                        var eventAttr = t.GetCustomAttributes().OfType<IProtocolEventAttribute>().SingleOrDefault();
                        if (eventAttr?.ProtocolType == protocolType)
                        {
                            if (!cachedDispatchers.TryGetValue(eventAttr.DispatcherType, out var dispatcher))
                            {
                                dispatcher = (IMessageDispatcher)Activator.CreateInstance(eventAttr.DispatcherType);
                                cachedDispatchers.Add(eventAttr.DispatcherType, dispatcher);
                            }
                            events.Add((t, null, dispatcher));
                        }
                    }
                    ret = new ProtocolInfo
                    {
                        Apis = apis.ToImmutableArray(),
                        Events = events.ToImmutableArray(),
                    };
                    _protocols.Add(protocolType, ret);
                }
                return ret;
            }
        }

        private struct SingleIdCheckResult
        {
            public string Key;
            public List<string> Values;
            public bool Enabled => Key is not null;
        }

        private static SingleIdCheckResult RunSingleIdCheck(ImmutableArray<(Type, Type, IMessageDispatcher)> list)
        {
            string singleIdKey = string.Empty; //Empty for not-initialized, null for failed.
            List<string> singleIdValue = new();
            foreach (var (dataType, _, dataDispatcher) in list)
            {
                if (!dataDispatcher.TryGetSingleDispatchId(dataType, out var pair))
                {
                    singleIdKey = null;
                    break;
                }
                singleIdValue.Add(pair.value);
                if (singleIdKey.Length == 0)
                {
                    singleIdKey = pair.key;
                }
                else if (singleIdKey != pair.key)
                {
                    singleIdKey = null;
                    break;
                }
            }
            return new()
            {
                Key = singleIdKey,
                Values = singleIdValue,
            };
        }

        //Note that this method is moved from event recv. Need to check for request recv.
        private static OnMessageMethodList EmitRecv(TypeBuilder type, SingleIdCheckResult sid,
            CodeGenFieldInitLists fieldInitLists, ImmutableArray<(Type, Type, IMessageDispatcher)> list,
            FieldBuilder protocolObj)
        {
            OnMessageMethodList ret = new();

            for (int i = 0; i < list.Length; ++i)
            {
                var (eventType, _, eventDispatcher) = list[i];
                //The event (and its accessors) are created by dispatcher.

                //private Task OnMessageXXX(MessageBuffer).
                var eventHandlerMethod = type.DefineMethod($"OnMessage{eventType.Name}", MethodAttributes.Private,
                    typeof(Task), new[] { typeof(MessageBuffer) });
                var il = eventHandlerMethod.GetILGenerator();
                var ilEnv = new ILGeneratorEnv(type, il, fieldInitLists);
                var retNullLabel = il.DefineLabel();

                int priority = 0;
                if (!sid.Enabled)
                {
                    //For multi-key mode, each message handler method need to check the dispatch keys.
                    var jsonDocumentLocal = il.DeclareLocal(typeof(JsonDocument));
                    il.Emit(OpCodes.Ldarg_1); //msgBuffer
                    il.Emit(OpCodes.Call, typeof(MessageBuffer).GetMethod(nameof(MessageBuffer.ToJsonDocument)));
                    il.Emit(OpCodes.Stloc, jsonDocumentLocal);

                    priority = eventDispatcher.EmitRecvMultiIdCheck(eventType, ilEnv, retNullLabel, jsonDocumentLocal);
                }
                ret.Add((eventHandlerMethod, priority, sid.Enabled ? sid.Values[i] : null));

                var msgBufferLocal = il.DeclareLocal(typeof(MessageBuffer));
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, msgBufferLocal);
                LocalBuilder protocolObjLocal = null;
                if (protocolObj is not null)
                {
                    protocolObjLocal = il.DeclareLocal(protocolObj.FieldType);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, protocolObj);
                    il.Emit(OpCodes.Stloc, protocolObjLocal);
                }
                eventDispatcher.EmitRecvAction(eventType, ilEnv, protocolObjLocal, msgBufferLocal);

                il.Emit(OpCodes.Ret); //Return the Task returned by the dispatcher.
                il.MarkLabel(retNullLabel);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ret); //Return null Task to indicate the multi-key check failed.
            }

            //Sort by priority (only needed by multi-id mode).
            ret.Sort((a, b) => a.priority - b.priority);

            return ret;
        }

        //Note that this method is moved from event recv. Need to check for request recv.
        private static MethodInfo EmitRecvEntry(TypeBuilder type, SingleIdCheckResult sid, CodeGenFieldInitLists fieldInitLists,
            OnMessageMethodList handlerMethods, out FieldBuilder dispatchMethodListField)
        {
            var ret = type.DefineMethod("OnMessage", MethodAttributes.Private, typeof(void), new[] { typeof(MessageBuffer) });
            {
                var il = ret.GetILGenerator();
                var ilEnv = new ILGeneratorEnv(type, il, fieldInitLists);

                if (sid.Enabled)
                {
                    //Single-id mode. Perform a lookup and directly call the handler.
                    dispatchMethodListField = type.DefineField("_sys_recvMethods", typeof(Func<MessageBuffer, Task>[]),
                        FieldAttributes.Private | FieldAttributes.InitOnly);

                    //if (!msgBuffer.ToJsonDocument().TryGetNestedProperty(key, out var value)) return;
                    var singleDispatchJsonValueField = il.DeclareLocal(typeof(string));
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, typeof(MessageBuffer).GetMethod(nameof(MessageBuffer.ToJsonDocument)));
                    il.Emit(OpCodes.Ldstr, sid.Key);
                    il.Emit(OpCodes.Ldloca, singleDispatchJsonValueField);
                    il.Emit(OpCodes.Call, typeof(DispatchKeyHelper).GetMethod(nameof(DispatchKeyHelper.TryGetNestedProperty)));
                    var contLabel = il.DefineLabel();
                    il.Emit(OpCodes.Brtrue, contLabel);
                    il.Emit(OpCodes.Ret);
                    il.MarkLabel(contLabel);

                    var lookupArray = handlerMethods.Select(m => m.val).ToArray();
                    var lookupArrayField = ilEnv.AddReadOnlyField("_sys_recvLookup", lookupArray);

                    //Lookup (using Array.IndexOf).
                    var indexLocal = il.DeclareLocal(typeof(int));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, lookupArrayField);
                    il.Emit(OpCodes.Ldloc, singleDispatchJsonValueField);
                    il.Emit(OpCodes.Call, typeof(Array).GetMethod(nameof(Array.IndexOf),
                        new[] { typeof(Array), typeof(object) }));
                    il.Emit(OpCodes.Stloc, indexLocal);

                    //Check not found (-1).
                    var retLabel = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, indexLocal);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.Emit(OpCodes.Beq, retLabel);

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, dispatchMethodListField); //Func<MessageBuffer, Task>[]
                    il.Emit(OpCodes.Ldloc, indexLocal);
                    il.Emit(OpCodes.Ldelem, typeof(string)); //delegate
                    il.Emit(OpCodes.Ldarg_1); //msgBuffer (arg1)
                    il.Emit(OpCodes.Call, typeof(Func<MessageBuffer, Task>).GetMethod("Invoke"));
                    il.Emit(OpCodes.Pop); //Dicard the Task.

                    il.MarkLabel(retLabel);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    dispatchMethodListField = null;

                    //Multi-id mode. Call each method in order until one returns non-null.
                    var retLabel = il.DefineLabel();
                    foreach (var (method, _, _) in handlerMethods)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Call, method);
                        il.Emit(OpCodes.Brtrue, retLabel);
                    }
                    il.MarkLabel(retLabel);
                    il.Emit(OpCodes.Ret);
                }
            }
            return ret;
        }

        //Used by generated ctor to initialize the single-id method list field (for message dispatching).
        private static void EmitInitSingleIdDispatchMethodListField(ILGenerator il, FieldBuilder field,
            OnMessageMethodList handlers)
        {
            var dispatchMethodListLocal = il.DeclareLocal(typeof(Func<MessageBuffer, Task>[]));
            il.Emit(OpCodes.Ldc_I4, handlers.Count);
            il.Emit(OpCodes.Newarr, typeof(Func<MessageBuffer, Task>));
            il.Emit(OpCodes.Stloc, dispatchMethodListLocal);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, dispatchMethodListLocal);
            il.Emit(OpCodes.Stfld, field);

            var funcCtor = typeof(Func<MessageBuffer, Task>)
                .GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            for (int i = 0; i < handlers.Count; ++i)
            {
                il.Emit(OpCodes.Ldloc, dispatchMethodListLocal);
                il.Emit(OpCodes.Ldc_I4, i);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldftn, handlers[i].method);
                il.Emit(OpCodes.Newobj, funcCtor);

                il.Emit(OpCodes.Stelem, typeof(Func<MessageBuffer, Task>));
            }
        }
    }
}
