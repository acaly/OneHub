using OneHub.Common.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class SetNotif
    {
        //TODO go-cqhttp has a more flexible way of defining this

        public List<string> ChannelIds { get; set; }
        public bool IncludePrivateChannels { get; set; }

        public sealed class Response
        {
        }
    }
}
