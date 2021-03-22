using OneHub.Common.Protocols.OneBot11.Objects;
using OneHub.Common.Protocols.OneX;
using OneHub.Common.Protocols.OneX.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupMessageSubType>))]
    public enum GroupMessageSubType
    {
        Unknown = 0,
        Normal = 1,
        Anonymous = 2,
        Notice = 3,
    }

    [OneBot11Event]
    public sealed class GroupMessage : OneBot11Event
    {
        public sealed class Response
        {
            public Message Reply { get; set; } = null;
            public bool AutoEscape { get; set; } = true;
            public bool AtSender { get; set; } = true;
            public bool Delete { get; set; } = false;
            public bool Kick { get; set; } = false;
            public bool Ban { get; set; } = false;
            public int BanDuration { get; set; } = 30 * 60;
        }

        [EventId]
        public string PostType => "Message";
        [EventId]
        public string MessageType => "Group";

        public GroupMessageSubType SubType { get; set; }
        public int MessageId { get; set; }
        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public AnonymousObject Anonymous { get; set; }
        public Message Message { get; set; }
        public string RawMessage { get; set; }
        public int Font { get; set; }
        public GroupMemberInfo Sender { get; set; }
    }
}
