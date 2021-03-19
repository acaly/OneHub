using OneHub.Common.Protocols.OneHub11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<MessageEventAction>))]
    public enum MessageEventAction
    {
        New = 1,
        Modified = 2,
        Deleted = 3,
    }

    [ProtocolEvent(ProtocolVersion.OneHub11)]
    public sealed class MessageReceived
    {
        [EventId]
        public string EventType => nameof(MessageReceived);

        public MessageEventAction MessageAction { get; set; }
        public MessageInfo Message { get; set; }
    }
}
