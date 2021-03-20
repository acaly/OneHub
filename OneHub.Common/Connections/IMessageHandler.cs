using OneHub.Common.Connections.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Connections
{
    public interface IMessageHandler
    {
        //TODO should not depend on web socket connection
        void Init(AbstractWebSocketConnection connection);
        bool CanHandle(MessageBuffer message);
        void SetResult(MessageBuffer message); //Should call message.Dispose after finish using to recycle.
        void Cancel();

        bool HasNextFilter { get; }
        IMessageHandler GetNextFilter();
    }
}
