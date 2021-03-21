using OneHub.Common.Connections;
using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OneHub11ApiRequestAttribute : MessageSerializerAttribute, IProtocolApiRequestAttribute
    {
        public Type ProtocolType => typeof(IOneHub11);
        public Type DispatcherType => typeof(OneXRequestDispatcher);

        public OneHub11ApiRequestAttribute() : base(typeof(OneXMessageSerializer<>))
        {
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OneHub11ApiResponseAttribute : MessageSerializerAttribute, IProtocolApiResponseAttribute
    {
        public Type ProtocolType => typeof(IOneHub11);

        public OneHub11ApiResponseAttribute() : base(typeof(OneXMessageSerializer<>))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OneHub11EventAttribute : MessageSerializerAttribute, IProtocolEventAttribute
    {
        public Type ProtocolType => typeof(IOneHub11);
        public Type DispatcherType => typeof(OneXEventDispatcher);

        public OneHub11EventAttribute() : base(typeof(OneXMessageSerializer<>))
        {
        }
    }
}
