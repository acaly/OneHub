using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class ProtocolEventAttribute : Attribute
    {
        public ProtocolVersion ProtocolVersion { get; }

        public ProtocolEventAttribute(ProtocolVersion protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }
    }
}
