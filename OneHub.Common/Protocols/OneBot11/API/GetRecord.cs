using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetRecord
    {
        public string File { get; set; }
        public string OutFormat { get; set; }

        public sealed class Response
        {
            public string File { get; set; }
        }
    }
}
