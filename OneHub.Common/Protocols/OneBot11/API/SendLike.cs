using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SendLike
    {
        public ulong UserId { get; set; }
        public int Times { get; set; } = 1;

        public sealed class Response
        {
        }
    }
}
