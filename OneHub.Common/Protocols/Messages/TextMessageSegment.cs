using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.Messages
{
    public sealed class TextMessageSegment : AbstractMessageSegment
    {
        public string Text { get; set; }

        internal override string GetSerializedType() => "text";
    }
}
