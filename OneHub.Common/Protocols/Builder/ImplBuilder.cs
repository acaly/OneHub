using OneHub.Common.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.Builder
{
    internal static class ImplBuilder
    {
        public static Func<object, IMessageHandler> BuildWebSocketImplFactory<T>(ProtocolBuilder.ProtocolInfo protocolInfo)
            where T : class
        {
            var options = JsonOptions.CreateSerializerOptions();

            Dictionary<string, Func<T, MessageBuffer, Task>> handlers = new();
            foreach (var action in protocolInfo.Apis)
            {
                var methodName = action.name + "Async";
                var method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method is null)
                {
                    throw new ProtocolBuilderException($"Api {action.name} is not defined in interface {typeof(T)}.");
                }
                var handler = EchoRequestHelper.MakeResponseHandler<T>(method, action.request, action.response, options);
                handlers.Add(JsonOptions.ConvertString(action.name), handler);
            }

            List<Action<V11ProtocolImplMessageHandler<T>, T>> subscriptions = new();
            foreach (var e in protocolInfo.Events)
            {
                var eventInfo = typeof(T).GetEvent(e.name);
                if (eventInfo is null)
                {
                    throw new ProtocolBuilderException($"Event {e.name} is not defined in interface {typeof(T)}.");
                }
                var helper = typeof(EchoRequestHelper).GetMethod(nameof(EchoRequestHelper.MakeEventSubscription), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(typeof(T), e.data);
                var subscription = (Action<V11ProtocolImplMessageHandler<T>, T>)helper.Invoke(null, new object[] { eventInfo, options });
                subscriptions.Add(subscription);
            }

            return obj => new V11ProtocolImplMessageHandler<T>((T)obj, handlers, subscriptions);
        }

        internal class V11ProtocolImplMessageHandler<T> : IMessageHandler where T : class
        {
            private readonly T _protocol;
            private readonly Dictionary<string, Func<T, MessageBuffer, Task>> _handlers;
            private readonly List<Action<V11ProtocolImplMessageHandler<T>, T>> _subscriptions;
            internal AbstractWebSocketConnection Connection { get; private set; }

            //TODO may need a sync obj per T protocol to avoid multiple handlers from handling the same binary message
            public V11ProtocolImplMessageHandler(T protocol,
                Dictionary<string, Func<T, MessageBuffer, Task>> handlers,
                List<Action<V11ProtocolImplMessageHandler<T>, T>> subscriptions)
            {
                _protocol = protocol;
                _handlers = handlers;
                _subscriptions = subscriptions;

                foreach (var s in subscriptions)
                {
                    s(this, protocol);
                }
            }

            public bool HasNextFilter => true;

            public IMessageHandler GetNextFilter()
            {
                return this;
            }

            public void Init(AbstractWebSocketConnection connection)
            {
                Connection = connection;
            }

            public bool CanHandle(MessageBuffer message)
            {
                var jsonDocument = message.ToJsonDocument();
                if (jsonDocument is null)
                {
                    return false;
                }
                if (!jsonDocument.RootElement.TryGetProperty("action", out var actionElement))
                {
                    return false;
                }
                return true;
            }

            public void SetResult(MessageBuffer message)
            {
                if (_handlers.TryGetValue(message.ToJsonDocument().RootElement.GetProperty("action").GetString(), out var h))
                {
                    _ = h(_protocol, message);
                }
            }

            public void Cancel()
            {
            }
        }
    }
}
