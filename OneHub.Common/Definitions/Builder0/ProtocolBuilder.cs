using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions.Builder0
{
    public static class ProtocolBuilder
    {
        internal class ProtocolInfo
        {
            public ImmutableArray<(string name, Type request, Type response)> Apis { get; init; }
            public ImmutableArray<(string name, Type data, ImmutableArray<(string key, string value)> ids)> Events { get; init; }
        }

        private class SequentialEqualityComparer<TList, TElement> : IEqualityComparer<TList> where TList : IEnumerable<TElement>
        {
            public static SequentialEqualityComparer<TList, TElement> Default { get; } = new();
            private readonly IEqualityComparer<TElement> _default = EqualityComparer<TElement>.Default;

            public bool Equals(TList x, TList y)
            {
                return Enumerable.SequenceEqual(x, y);
            }

            public int GetHashCode(TList obj)
            {
                int hashCode = 0;
                foreach (var e in obj)
                {
                    HashCode.Combine(hashCode, _default.GetHashCode(e));
                }
                return hashCode;
            }
        }

        private static readonly Dictionary<Type, ProtocolInfo> _protocols = new();
        private static readonly Dictionary<Type, Func<object, IMessageHandler>> _wsImpl = new();
        private static readonly Dictionary<Type, Func<AbstractWebSocketConnection, object>> _wsInterface = new();

        private static ProtocolInfo GetProtocolInfo(Type protocolType)
        {
            lock (_protocols)
            {
                if (!_protocols.TryGetValue(protocolType, out var ret))
                {
                    List<(string name, Type request, Type response)> apis = new();
                    List<(string name, Type data, ImmutableArray<(string key, string value)> ids)> events = new();
                    var eventIdBuilder = ImmutableArray.CreateBuilder<(string key, string value)>();

                    foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
                    {
                        if (t.GetCustomAttributes().OfType<IProtocolApiRequestAttribute>().SingleOrDefault()?.ProtocolType == protocolType)
                        {
                            var responseType = t.GetNestedType("Response");
                            //TODO check response attribute
                            if (responseType is null)
                            {
                                throw new ProtocolBuilderException($"Api definition {t} does not have a response type.");
                            }
                            apis.Add((t.Name, t, responseType));
                        }
                        if (t.GetCustomAttributes().OfType<IProtocolEventAttribute>().SingleOrDefault()?.ProtocolType == protocolType)
                        {
                            if (typeof(IBinaryMixedObject).IsAssignableFrom(t))
                            {
                                throw new ProtocolBuilderException($"Event {t} cannot be IBinaryMixedObject.");
                            }
                            FindEventIds(t, eventIdBuilder);
                            events.Add((t.Name, t, eventIdBuilder.ToImmutable()));
                        }
                    }

                    //Increase priority of events with more [EventId]s.
                    events.Sort((e1, e2) => e2.ids.Length - e1.ids.Length);

                    ret = new ProtocolInfo()
                    {
                        Apis = apis.ToImmutableArray(),
                        Events = events.ToImmutableArray(),
                    };
                    CheckProtocol(ret);
                    _protocols.Add(protocolType, ret);
                }
                return ret;
            }
        }

        private static void FindEventIds(Type type, ImmutableArray<(string key, string value)>.Builder builder)
        {
            builder.Clear();
            var instance = Activator.CreateInstance(type); //Used to read the instance properties.
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
                builder.Add((JsonOptions.ConvertString(property.Name), (string)property.GetValue(instance)));
            }
            if (builder.Count == 0)
            {
                throw new ProtocolBuilderException($"Event {type} does not have event id.");
            }
        }

        private static void CheckProtocol(ProtocolInfo protocolInfo)
        {
            //Api name confliction.
            var apiNames = protocolInfo.Apis.Select(api => api.name).Distinct();
            if (apiNames.Count() != protocolInfo.Apis.Length)
            {
                throw new ProtocolBuilderException("Api name confliction.");
            }

            //Event name confliction (InterfaceBuilder assumes event names are different).
            var eventNames = protocolInfo.Events.Select(e => e.name).Distinct();
            if (eventNames.Count() != protocolInfo.Events.Length)
            {
                throw new ProtocolBuilderException("Event name confliction.");
            }

            //Event id confliction.
            var eventIds = protocolInfo.Events.Select(e => e.ids)
                .Distinct(SequentialEqualityComparer<ImmutableArray<(string, string)>, (string, string)>.Default);
            if (eventIds.Count() != protocolInfo.Events.Length)
            {
                throw new ProtocolBuilderException("Event ID confliction.");
            }
        }

        //Note that this will add a ServerMessageHandler (although being the client) to handle events from server.
        //Currently this handler cannot be removed. This is the issue from the implementation of AbstractWebSocketConnection
        //which does not support cancelling an added handler.
        public static T BuildWebSocketInterface<T>(AbstractWebSocketConnection connection)
            where T : class
        {
            if (!_wsInterface.TryGetValue(typeof(T), out var factory))
            {
                factory = InterfaceBuilder.BuildWebSocketInterfaceFactory(typeof(T), GetProtocolInfo(typeof(T)));
                _wsInterface[typeof(T)] = factory;
            }
            return (T)factory(connection);
        }

        public static IMessageHandler BuildWebSocketImpl<TInterface, TImpl>(TImpl obj)
            where TInterface : class
            where TImpl : class, TInterface
        {
            lock (_wsImpl)
            {
                if (!_wsImpl.TryGetValue(typeof(TImpl), out var p))
                {
                    p = ImplBuilder.BuildWebSocketImplFactory<TImpl>(GetProtocolInfo(typeof(TInterface)));
                    _wsImpl[typeof(TImpl)] = p;
                }
                return p(obj);
            }
        }
    }
}
