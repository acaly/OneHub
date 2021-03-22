using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SetGroupLeave
    {
        public ulong GroupId { get; set; }
        public bool IsDismiss { get; set; } = false;

        public sealed class Response
        {
        }
    }
}
