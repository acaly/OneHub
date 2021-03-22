using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OneBot11ApiRequestAttribute : OneXApiRequestAttribute
    {
        public OneBot11ApiRequestAttribute() : base(typeof(IOneBot11))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OneBot11EventAttribute : OneXEventAttribute
    {
        public OneBot11EventAttribute() : base(typeof(IOneBot11))
        {
        }
    }
}
