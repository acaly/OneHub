using OneHub.Common.WebSockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.Builder
{
    //This is only used by the dynamically generated module.
    //Be careful when using in other places: the message handler method must finish synchronously (buffer will be disposed).
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InterfaceServerMessageHandler : AbstractMessageHandler
    {
        private class NextHandlerHolder
        {
            public InterfaceServerMessageHandler Handler;
        }

        //Here we convet an Action into an async Func<..., ValueTask> and then converted back into an Action.
        //Maybe we should save this by directly implementing IMessageHandler.
        public InterfaceServerMessageHandler(Action<MessageBuffer> task)
            : base(async msgTask => task(await msgTask), CreateHolder(out var holder))
        {
            holder.Handler = this;
        }

        private static Func<InterfaceServerMessageHandler> CreateHolder(out NextHandlerHolder ret)
        {
            var r = new NextHandlerHolder();
            ret = r;
            return () => r.Handler;
        }

        public override bool CanHandle(MessageBuffer message)
        {
            return true;
        }
    }
}
