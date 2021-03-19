using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.WebSockets
{
    public interface IBinaryMixedObject
    {
        MemoryStream Stream { get; set; }

        internal class Helper<T>
        {
            public static bool IsBinaryMixed { get; } = typeof(IBinaryMixedObject).IsAssignableFrom(typeof(T));
        }
    }
}
