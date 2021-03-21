using OneHub.Common.Connections;
using OneHub.Common.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class OneXProtocolAttribute : Attribute, IUseBinaryMessageDecoderAttribute
    {
        public Type DecoderType => typeof(OneXBinaryDecoder);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OneXApiRequestAttribute : Attribute, IProtocolApiRequestAttribute
    {
        public Type ProtocolType { get; }
        public Type DispatcherType => typeof(OneXRequestDispatcher);

        public OneXApiRequestAttribute(Type protocolType)
        {
            ProtocolType = protocolType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OneXEventAttribute : MessageSerializerAttribute, IProtocolEventAttribute
    {
        public Type ProtocolType { get; }
        public Type DispatcherType => typeof(OneXEventDispatcher);

        public OneXEventAttribute(Type protocolType) : base(typeof(JsonOptions.TextMessageSerializer<>))
        {
            ProtocolType = protocolType;
        }
    }
}
