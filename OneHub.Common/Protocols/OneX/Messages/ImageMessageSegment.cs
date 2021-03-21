using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX.Messages
{
    public sealed class ImageMessageSegment : AbstractMessageSegment
    {
        public string File { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Cache { get; set; }
        public string Proxy { get; set; }
        public string Timeout { get; set; }

        internal override string GetSerializedType() => "image";
    }
}
