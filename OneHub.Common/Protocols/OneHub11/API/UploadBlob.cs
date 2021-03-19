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
    public sealed class UploadBlob : IBinaryMixedObject
    {
        public MemoryStream Stream { get; set; }

        public sealed class Response
        {
            public string BlobId { get; set; }
        }
    }
}
