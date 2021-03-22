using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupRequestSubType>))]
    public enum GroupRequestSubType
    {
        Unknown = 0,
        Add = 1,
        Invite = 2,
    }

    [OneBot11Event]
    public sealed class GroupRequest : OneBot11Event
    {
        public sealed class Response
        {
            public bool? Approve { get; set; } = null;
            public string Reason { get; set; }
        }

        [EventId]
        public string PostType => "Request";
        [EventId]
        public string RequestType => "Group";

        public GroupRequestSubType SubType { get; set; }
        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public string Comment { get; set; }
        public string Flag { get; set; }
    }
}
