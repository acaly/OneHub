using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupBanSubType>))]
    public enum GroupBanSubType
    {
        Unknown,
        Ban,
        LiftBan,
    }

    [OneBot11Event]
    public sealed class GroupBan : OneBot11Event
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "GroupBan";

        public GroupBanSubType SubType { get; set; }
        public ulong GroupId { get; set; }
        public ulong OperatorId { get; set; }
        public ulong UserId { get; set; }
        public int Duration { get; set; }
    }
}
