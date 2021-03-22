using OneHub.Common.Protocols.OneX.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetForwardMsg
    {
        public string Id { get; set; }

        public sealed class Response
        {
            public Message Message { get; set; }
        }
    }
}
