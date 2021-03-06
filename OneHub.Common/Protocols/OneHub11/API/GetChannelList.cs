using OneHub.Common.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class GetChannelList
    {
        public sealed class Response
        {
            public List<GetChannelInfo> Channels { get; set; }
        }
    }
}
