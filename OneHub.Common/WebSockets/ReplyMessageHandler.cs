using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.WebSockets
{
    public sealed class ReplyMessageHandler<T> : AbstractMessageHandler<T> where T : class
    {
        private readonly Func<MessageBuffer, bool> _canHandle;

        public ReplyMessageHandler(Func<MessageBuffer, bool> canHandle, Func<ValueTask<T>, ValueTask> task,
            JsonSerializerOptions options)
            : base(task, options)
        {
            _canHandle = canHandle;
        }

        public override bool CanHandle(MessageBuffer message)
        {
            return _canHandle(message);
        }
    }

    public static class WebSocketConnectionReplyMessageExtensions
    {
        public static async Task<TReply> SendJsonMessageAsync<TMessage, TReply>(this AbstractWebSocketConnection connection,
            TMessage message, Func<MessageBuffer, bool> filter, JsonSerializerOptions options)
            where TMessage : class
            where TReply : class
        {
            var taskSource = new TaskCompletionSource<TReply>();
            connection.AddMessageHandler(new ReplyMessageHandler<TReply>(filter, async replyTask =>
            {
                try
                {
                    var reply = await replyTask;
                    taskSource.SetResult(reply);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            }, options));
            await connection.SendJsonMessageAsync(message, options);
            return await taskSource.Task;
        }
    }
}
