using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupDecreaseSubType>))]
    public enum GroupDecreaseSubType
    {
        Unknown = 0,
        Leave = 1,
        Kick = 2,
        KickMe = 3,
    }

    [OneBot11Event]
    public sealed class GroupDecrease : OneBot11Event
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "GroupDecrease";

        public GroupDecreaseSubType SubType { get; set; }
        public ulong GroupId { get; set; }
        public ulong OperatorId { get; set; }
        public ulong UserId { get; set; }
    }
}
