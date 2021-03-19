using OneHub.Common.WebSockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [ProtocolApi(ProtocolVersion.OneHub11)]
    public sealed class DownloadBlob
    {
        public string BlobId { get; set; }
        public string OutputFormat { get; set; }

        public sealed class Response : IBinaryMixedObject
        {
            public MemoryStream Stream { get; set; }
        }
    }
}
