using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public sealed class ProtocolBuilderException : Exception
    {
        public ProtocolBuilderException(string message) : base(message)
        {
        }
    }
}
