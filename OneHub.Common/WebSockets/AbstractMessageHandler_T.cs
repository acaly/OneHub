using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.WebSockets
{
    public abstract class AbstractMessageHandler<T> : AbstractMessageHandler where T : class
    {
        protected AbstractMessageHandler(Func<ValueTask<T>, ValueTask> task, JsonSerializerOptions jsonOptions,
            Func<IMessageHandler> nextHandler = null)
            : base(CreateHandlerFunc(task, jsonOptions), nextHandler)
        {
        }

        private static Func<ValueTask<MessageBuffer>, ValueTask> CreateHandlerFunc(Func<ValueTask<T>, ValueTask> task,
            JsonSerializerOptions jsonOptions)
        {
            return async msgTask =>
            {
                ValueTask<T> t;
                try
                {
                    var msg = await msgTask;
                    if (IBinaryMixedObject.Helper<T>.IsBinaryMixed)
                    {
                        var stream = new MemoryStream();
                        var obj = msg.ReadJsonBinary<T>(stream, jsonOptions);
                        ((IBinaryMixedObject)(object)obj).Stream = stream;
                        t = ValueTask.FromResult(obj);
                    }
                    else
                    {
                        t = ValueTask.FromResult(msg.ReadJson<T>(jsonOptions));
                    }
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
