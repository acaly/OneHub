using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetCookies
    {
        public string Domain { get; set; } = string.Empty;

        public sealed class Response
        {
            public string Cookies { get; set; }
        }
    }
}
