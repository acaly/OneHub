using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Connections
{
    public interface IMessageSerializer<T>
    {
        void Serialize(MessageBuffer messageBuffer, T obj);
        T Deserialize(MessageBuffer messageBuffer);
    }

    public class MessageSerializer<T> : IMessageSerializer<T>
    {
        protected JsonSerializerOptions Options { get; }

        public MessageSerializer(JsonSerializerOptions options)
        {
            Options = options;
        }

        public virtual void Serialize(MessageBuffer messageBuffer, T obj)
        {
            messageBuffer.WriteJson(obj, Options);
        }

        public virtual T Deserialize(MessageBuffer messageBuffer)
        {
            return messageBuffer.ReadJson<T>(Options);
        }
    }
}
