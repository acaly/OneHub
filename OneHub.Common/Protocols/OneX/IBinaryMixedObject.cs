using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    public interface IBinaryMixedObject
    {
        MemoryStream Stream { get; set; }
    }
}
