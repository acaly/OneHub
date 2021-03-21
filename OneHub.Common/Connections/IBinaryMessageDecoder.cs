using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Connections
{
    public interface IBinaryMessageDecoder
    {
        //Try to deserialize a json from a message buffer's MemoryStream.
        bool TryDecode(MessageBuffer messageBuffer, out JsonDocument document);
    }
}
