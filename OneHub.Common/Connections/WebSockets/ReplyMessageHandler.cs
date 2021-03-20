using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Connections.WebSockets
{
    public sealed class ReplyMessageHandler<T> : AbstractMessageHandler<T> where T : class
    {
        private readonly Func<MessageBuffer, bool> _canHandle;

        public ReplyMessageHandler(Func<MessageBuffer, bool> canHandle, Func<ValueTask<T>, ValueTask> task)
            : base(task)
        {
            _canHandle = canHandle;
        }

        public override bool CanHandle(MessageBuffer message)
        {
            return _canHandle(message);
        }
    }
}
