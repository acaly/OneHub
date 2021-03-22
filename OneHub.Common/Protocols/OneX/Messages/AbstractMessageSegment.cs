using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX.Messages
{
    [JsonConverter(typeof(MessageSegmentConverter))]
    public abstract class AbstractMessageSegment
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract string SerializedType { get; }

        public abstract string GetDisplayedText();
    }
}
