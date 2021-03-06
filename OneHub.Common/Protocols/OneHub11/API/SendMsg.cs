using OneHub.Common.Protocols.OneX.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class SendMsg
    {
        public string MessageId { get; set; } //optional
        public string ChannelId { get; set; }
        public string MessageType { get; set; }
        public Message Message { get; set; }

        public sealed class Response
        {
            public string MessageId { get; set; }
        }
    }
}
