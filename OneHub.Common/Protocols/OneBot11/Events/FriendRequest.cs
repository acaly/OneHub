using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [OneBot11Event]
    public sealed class FriendRequest : OneBot11Event
    {
        public sealed class Response
        {
            public bool? Approve { get; set; } = null;
            public string Remark { get; set; } = string.Empty;
        }

        [EventId]
        public string PostType => "Request";
        [EventId]
        public string RequestType => "FriendRequest";

        public ulong UserId { get; set; }
        public string Comment { get; set; }
        public string Flag { get; set; }
    }
}
