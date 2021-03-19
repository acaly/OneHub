using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.WebSockets
{
    public sealed class ServerMessageHandler<T> : AbstractMessageHandler<T> where T : class
    {
        private readonly Func<MessageBuffer, bool> _canHandle;

        public ServerMessageHandler(Func<MessageBuffer, bool> canHandle, Func<ValueTask<T>, ValueTask> task,
            JsonSerializerOptions options)
            : base(task, options, () => new ServerMessageHandler<T>(canHandle, task, options))
        {
            _canHandle = canHandle;
        }

        public override bool CanHandle(MessageBuffer message)
        {
            return _canHandle(message);
        }
    }
}
