using OneHub.Common.Protocols.Messages;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    public sealed class PrivateMessage
    {
        public sealed class SenderInfo
        {
            public ulong UserId { get; set; }
            public string Nickname { get; set; }
            public string Sex { get; set; }
            public int Age { get; set; }
        }

        public sealed class Response
        {
            public List<AbstractMessageSegment> Reply { get; set; }
            public bool AutoEscape { get; set; }
        }

        [EventId]
        public string PostType { get; } = "Message";
        [EventId]
        public string MessageType { get; } = "Private";

        //TODO use DateTime through JsonConverter
        public long Time { get; set; }
        public long SelfId { get; set; }
        public string SubType { get; set; }
        public int MessageId { get; set; }
        public ulong UserId { get; set; }
        public Message Message { get; set; }
        public string RawMessage { get; set; }
        public int Font { get; set; }
        public SenderInfo Sender { get; set; }
    }
}
