using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [OneBot11Event]
    public sealed class GroupRecall : OneBot11Event
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "GroupRecall";

        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public ulong OperatorId { get; set; }
        public int MessageId { get; set; }
    }
}
