using OneHub.Common.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolApiRequestAttribute : MessageSerializerAttribute
    {
        public Type ProtocolType { get; }

        public ProtocolApiRequestAttribute(Type protocolType, Type serializerType) : base(serializerType)
        {
            ProtocolType = protocolType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolApiResponseAttribute : MessageSerializerAttribute
    {
        public Type ProtocolType { get; }

        public ProtocolApiResponseAttribute(Type protocolType, Type serializerType) : base(serializerType)
        {
            ProtocolType = protocolType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ProtocolEventAttribute : MessageSerializerAttribute
    {
        public Type ProtocolType { get; }

        public ProtocolEventAttribute(Type protocolType, Type serializerType) : base(serializerType)
        {
            ProtocolType = protocolType;
        }
    }
}
