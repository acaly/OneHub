using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.WebSockets
{
    public interface IMessageHandler
    {
        void Init(AbstractWebSocketConnection connection);
        bool CanHandle(MessageBuffer message);
        void SetResult(MessageBuffer message); //Should call message.Dispose after finish using to recycle.
        void Cancel();

        bool HasNextFilter { get; }
        IMessageHandler GetNextFilter();
    }
}
