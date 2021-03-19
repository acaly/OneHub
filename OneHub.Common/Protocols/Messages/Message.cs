using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.Messages
{
    [JsonConverter(typeof(MessageConverter))]
    public class Message
    {
        public List<AbstractMessageSegment> Segments { get; set; }
    }
}
