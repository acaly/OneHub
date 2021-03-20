using OneHub.Common.Connections;
using OneHub.Common.Connections.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Definitions.Builder0
{
    public static class WebSocketConnectionJsonExtensions
    {
        public static Task SendJsonMessageAsync<T>(this AbstractWebSocketConnection connection, T data)
        {
            var msg = connection.CreateMessageBuffer();
            MessageSerializer.Serialize(msg, data);
            return connection.SendMessageAsync(msg);
        }

        public static async Task<TReply> SendJsonMessageAsync<TMessage, TReply>(this AbstractWebSocketConnection connection,
            TMessage message, Func<MessageBuffer, bool> filter)
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
            }));
            await connection.SendJsonMessageAsync(message);
            return await taskSource.Task;
        }
    }
}
