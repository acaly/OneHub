using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupAdminEventSubType>))]
    public enum GroupAdminEventSubType
    {
        Set,
        Unset,
    }

    [OneBot11Event]
    public sealed class GroupAdmin : OneBot11Event
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "GroupAdmin";

        public GroupAdminEventSubType SubType { get; set; }
        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
    }
}
