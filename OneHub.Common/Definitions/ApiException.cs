using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public class ApiException : Exception
    {
        public string Api { get; init; }
        public int Code { get; init; }
    }
}
