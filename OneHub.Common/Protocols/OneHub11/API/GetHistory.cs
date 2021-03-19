using OneHub.Common.Protocols.OneHub11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [ProtocolApi(ProtocolVersion.OneHub11)]
    public sealed class GetHistory
    {
        public string ChannelId { get; set; }
        [Obsolete("use JsonConverter")]
        public DateTime Since { get; set; }
        public int Max { get; set; }

        public RequestContentType RequestContent { get; set; }
        public RequestChildrenType RequestChildren { get; set; }

        public sealed class Response
        {
            public List<MessageInfo> Messages { get; set; }
        }
    }
}
