using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetVersionInfo
    {
        public sealed class Response
        {
            public string AppName { get; set; }
            public string AppVersion { get; set; }
            public string ProtocolVersion { get; set; }
        }
    }
}
