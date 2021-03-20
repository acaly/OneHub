using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Connections.WebSockets
{
    public interface IWebSocketMessageHandler
    {
        //return true if handled; false otherwise
        ValueTask<bool> HandleMessageAsync(AbstractWebSocketConnection connection, MessageBuffer message);
    }
}
