using OneHub.Common.Protocols.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Objects
{
    [JsonConverter(typeof(Enum32JsonConverter<MessageAccessibilities>))]
    [Flags]
    public enum MessageAccessibilities
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
    }

    public sealed class MessageInfo
    {
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public string UserId { get; set; }
        public string RootMessageId { get; set; }

        public string MessageType { get; set; } //private, group, post
        public string MessageSubType { get; set; } //impl-defined

        public Message Message { get; set; }

        public MessageAccessibilities Accessibilities { get; set; }

        [Obsolete("use JsonConverter")]
        public DateTime Time { get; set; }
        [Obsolete("use JsonConverter")]
        public DateTime LastModified { get; set; }
    }
}
