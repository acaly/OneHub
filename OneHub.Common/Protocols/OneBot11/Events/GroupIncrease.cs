using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupIncreaseSubType>))]
    public enum GroupIncreaseSubType
    {
        Unknown = 0,
        Approve = 1,
        Invite = 2,
    }

    [OneBot11Event]
    public sealed class GroupIncrease : OneBot11Event
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "GroupIncrease";

        public GroupIncreaseSubType SubType { get; set; }
        public ulong GroupId { get; set; }
        public ulong OperatorId { get; set; }
        public ulong UserId { get; set; }
    }
}
