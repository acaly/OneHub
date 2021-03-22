using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SetGroupWholeBan
    {
        public ulong GroupId { get; set; }
        public bool Enabled { get; set; } = true;

        public sealed class Response
        {
        }
    }
}
