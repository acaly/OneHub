using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [OneBot11Event]
    public sealed class LuckyKingNotity
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "Notify";
        [EventId]
        public string SubType => "LuckyKing";

        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public ulong TargetId { get; set; }
    }
}
