using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX.Messages
{
    public sealed class UnknownMessageSegment : AbstractMessageSegment
    {
        public string Type { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public override string SerializedType => Type;
        public override string GetDisplayedText() => "[不支持的内容]";
    }
}
