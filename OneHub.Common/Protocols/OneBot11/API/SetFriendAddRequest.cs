using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SetFriendAddRequest
    {
        public string Flag { get; set; }
        public bool Approve { get; set; } = true;
        public string Remark { get; set; } = string.Empty;

        public sealed class Response
        {
        }
    }
}
