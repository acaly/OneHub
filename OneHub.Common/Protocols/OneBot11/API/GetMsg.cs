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
    public sealed class GetMsg
    {
        public int MessageId { get; set; }

        public sealed class Response
        {
            public int Time { get; set; }
            public MessageType MessageType { get; set; }
            public int MessageId { get; set; }
            public int RealId { get; set; }
            public UserInfo Sender { get; set; }
            public Message Message { get; set; }
        }
    }
}
