using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [ProtocolApi(ProtocolVersion.OneHub11)]
    public sealed class GetApiCapability
    {
        public sealed class Response
        {
            public List<string> Capabilities { get; set; }
        }
    }
}
