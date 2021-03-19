using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [ProtocolApi(ProtocolVersion.OneHub11)]
    public sealed class SetNotif
    {
        public List<string> ChannelIds { get; set; }
        public bool IncludePrivateChannels { get; set; }

        public sealed class Response
        {
        }
    }
}
