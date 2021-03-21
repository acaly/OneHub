using OneHub.Common.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions
{
    public interface IProtocolApiRequestAttribute
    {
        Type ProtocolType { get; }
        Type DispatcherType { get; }
    }

    public interface IProtocolEventAttribute
    {
        Type ProtocolType { get; }
        Type DispatcherType { get; }
    }
}
