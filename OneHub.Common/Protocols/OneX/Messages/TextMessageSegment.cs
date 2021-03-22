using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX.Messages
{
    public sealed class TextMessageSegment : AbstractMessageSegment
    {
        public string Text { get; set; }

        public override string SerializedType => "text";
        public override string GetDisplayedText() => Text;
    }
}
