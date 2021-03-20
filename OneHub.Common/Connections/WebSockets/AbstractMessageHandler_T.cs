using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.Connections.WebSockets
{
    public abstract class AbstractMessageHandler<T> : AbstractMessageHandler where T : class
    {
        protected AbstractMessageHandler(Func<ValueTask<T>, ValueTask> task, Func<IMessageHandler> nextHandler = null)
            : base(CreateHandlerFunc(task), nextHandler)
        {
        }

        private static Func<ValueTask<MessageBuffer>, ValueTask> CreateHandlerFunc(Func<ValueTask<T>, ValueTask> task)
        {
            return async msgTask =>
            {
                ValueTask<T> t;
                try
                {
                    var msg = await msgTask;
                    t = ValueTask.FromResult(MessageSerializer.Deserialize<T>(msg));
                }
                catch (Exception e)
                {
                    t = ValueTask.FromException<T>(e);
                }
                await task(t);
            };
        }
    }
}
