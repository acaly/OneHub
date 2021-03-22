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
    public sealed class OneHub11ApiRequestAttribute : OneXApiRequestAttribute
    {
        public OneHub11ApiRequestAttribute() : base(typeof(IOneHub11))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OneHub11EventAttribute : OneXEventAttribute
    {
        public OneHub11EventAttribute() : base(typeof(IOneHub11))
        {
        }
    }
}
