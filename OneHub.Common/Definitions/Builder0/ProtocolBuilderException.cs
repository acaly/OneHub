using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions.Builder0
{
    public sealed class ProtocolBuilderException : Exception
    {
        public ProtocolBuilderException(string message) : base(message)
        {
        }
    }
}
