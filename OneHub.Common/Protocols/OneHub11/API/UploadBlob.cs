using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class UploadBlob : IBinaryMixedObject
    {
        [JsonIgnore]
        public MemoryStream Stream { get; set; }

        public sealed class Response
        {
            public string BlobId { get; set; }
        }
    }
}
