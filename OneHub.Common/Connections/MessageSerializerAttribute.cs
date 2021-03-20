using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Connections
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageSerializerAttribute : Attribute
    {
        public Type SerializerType { get; }

        public MessageSerializerAttribute(Type serializerType)
        {
            SerializerType = serializerType;
        }
    }
}
