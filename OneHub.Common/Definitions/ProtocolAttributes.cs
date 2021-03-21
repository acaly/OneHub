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

    //This is actually never used.
    public interface IProtocolApiResponseAttribute
    {
        Type ProtocolType { get; }
    }

    public interface IProtocolEventAttribute
    {
        Type ProtocolType { get; }
        Type DispatcherType { get; }
    }
}
