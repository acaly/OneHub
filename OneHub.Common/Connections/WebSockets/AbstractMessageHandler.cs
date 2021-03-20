using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneHub.Common.Connections.WebSockets
{
    [SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly",
        Justification = "ValueTasks are only used to wrap cancellation and exception.")]
    public abstract class AbstractMessageHandler : IMessageHandler
    {
        private readonly Func<ValueTask<MessageBuffer>, ValueTask> _task;
        private readonly Func<IMessageHandler> _nextHandler;

        public AbstractMessageHandler(Func<ValueTask<MessageBuffer>, ValueTask> task, Func<IMessageHandler> nextHandler = null)
        {
            _task = task;
            _nextHandler = nextHandler;
        }

        public bool HasNextFilter => _nextHandler is not null;

        public virtual void Cancel()
        {
            _ = _task(ValueTask.FromCanceled<MessageBuffer>(new CancellationToken(true)));
        }

        public IMessageHandler GetNextFilter()
        {
            if (_nextHandler is null)
            {
                throw new NotSupportedException();
            }
            return _nextHandler();
        }

        public abstract bool CanHandle(MessageBuffer message);

        public virtual void Init(AbstractWebSocketConnection connection)
        {
        }

        protected virtual async Task HandleMessageAsync(MessageBuffer message)
        {
            //Force the task to run asynchronously.
            await Task.Yield();

            try
            {
                if (message is null)
                {
                    await _task(ValueTask.FromCanceled<MessageBuffer>(new CancellationToken(true)));
                }
                else
                {
                    await _task(ValueTask.FromResult(message));
                    message.Dispose();
                }
            }
            catch (Exception e2)
            {
                //TODO log
            }
        }

        public void SetResult(MessageBuffer message)
        {
            //Start and ignore returned task.
            _ = HandleMessageAsync(message);
        }
    }
}
