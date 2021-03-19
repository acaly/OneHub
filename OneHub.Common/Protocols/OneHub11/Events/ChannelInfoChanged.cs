﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Events
{
    [ProtocolEvent(ProtocolVersion.OneHub11)]
    public sealed class ChannelInfoChanged
    {
        [EventId]
        public string EventType => nameof(ChannelInfoChanged);

        public string ChannelId { get; set; }
    }
}
