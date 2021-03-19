using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Events
{
    [ProtocolEvent(ProtocolVersion.OneHub11)]
    public sealed class GroupMemberChanged
    {
        [EventId]
        public string EventType => nameof(GroupMemberChanged);

        public string GroupId { get; set; }
    }
}
