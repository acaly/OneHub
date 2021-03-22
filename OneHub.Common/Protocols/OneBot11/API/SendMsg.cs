using OneHub.Common.Protocols.OneBot11.Objects;
using OneHub.Common.Protocols.OneX.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SendMsg
    {
        public MessageType MessageType { get; set; }
        public ulong UserId { get; set; }
        public ulong GroupId { get; set; }
        public Message Message { get; set; }
        public bool AutoEscape { get; set; } = false;

        public sealed class Response
        {
            public int MessageId { get; set; }
        }
    }
}
