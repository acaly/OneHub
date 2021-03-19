using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.Messages
{
    public sealed class UnknownMessageSegment : AbstractMessageSegment
    {
        public string Type { get; set; }
        public Dictionary<string, string> Data { get; set; }

        internal override string GetSerializedType() => Type;
    }
}
