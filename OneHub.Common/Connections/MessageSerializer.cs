using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Connections
{
    public static class MessageSerializer
    {
        private static readonly ConcurrentDictionary<Type, object> _serializers = new();

        public static void Serialize<T>(MessageBuffer messageBuffer, T obj)
        {
            var s = (IMessageSerializer<T>)_serializers.GetOrAdd(typeof(T), CreateSerializer);
            s.Serialize(messageBuffer, obj);
        }

        public static T Deserialize<T>(MessageBuffer messageBuffer)
        {
            var s = (IMessageSerializer<T>)_serializers.GetOrAdd(typeof(T), CreateSerializer);
            return s.Deserialize(messageBuffer);
        }

        private static object CreateSerializer(Type t)
        {
            var s = t.GetCustomAttribute<MessageSerializerAttribute>()?.SerializerType;
            if (s.IsGenericTypeDefinition)
            {
                s = s.MakeGenericType(t);
            }
            return Activator.CreateInstance(s);
        }
    }
}
