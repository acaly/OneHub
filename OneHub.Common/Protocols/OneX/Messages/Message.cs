using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX.Messages
{
    [JsonConverter(typeof(MessageConverter))]
    public class Message
    {
        public List<AbstractMessageSegment> RawSegments { get; set; }
        public string RawString { get; set; }

        public List<AbstractMessageSegment> GetSegments(bool useRawString = true)
        {
            if (useRawString)
            {
                if (RawString is not null)
                {
                    return new()
                    {
                        new TextMessageSegment { Text = RawString },
                    };
                }
                else
                {
                    //Here RawSegments may be null, but it's the expected behavior.
                    return RawSegments;
                }
            }
            else
            {
                if (RawSegments is null)
                {
                    throw new JsonException("Cannot parse message code");
                }
                return RawSegments;
            }
        }

        public override string ToString()
        {
            var segments = GetSegments(useRawString: true);
            if (segments is null)
            {
                return "<null>";
            }
            var sb = new StringBuilder();
            foreach (var s in segments)
            {
                sb.Append(s.GetDisplayedText());
            }
            return sb.ToString();
        }
    }
}
