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
    [JsonConverter(typeof(Enum32JsonConverter<PrivateMessageSubType>))]
    public enum PrivateMessageSubType
    {
        Unknown = 0,
        Friend = 1,
        Group = 2,
        Other = 3,
    }

    [OneBot11Event]
    public sealed class PrivateMessage : OneBot11Event
    {
        public sealed class Response
        {
            public Message Reply { get; set; }
            public bool AutoEscape { get; set; }
        }

        [EventId]
        public string PostType => "Message";
        [EventId]
        public string MessageType => "Private";

        public PrivateMessageSubType SubType { get; set; }
        public int MessageId { get; set; }
        public ulong UserId { get; set; }
        public Message Message { get; set; }
        public string RawMessage { get; set; }
        public int Font { get; set; }
        public UserInfo Sender { get; set; }
    }
}
