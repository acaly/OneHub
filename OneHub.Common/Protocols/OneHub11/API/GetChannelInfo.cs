using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneHub11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class GetChannelInfo
    {
        public string ChannelId { get; set; }

        public sealed class Response
        {
            public ChannelInfo Channel { get; set; }
        }
    }
}
