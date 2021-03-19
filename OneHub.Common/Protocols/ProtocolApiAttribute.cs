using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class ProtocolApiAttribute : Attribute
    {
        public ProtocolVersion ProtocolVersion { get; }

        public ProtocolApiAttribute(ProtocolVersion protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }
    }
}
