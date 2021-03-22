using OneHub.Common.Protocols.OneBot11.Objects;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [OneBot11Event]
    public sealed class Heartbeat : OneBot11Event
    {
        [EventId]
        public string PostType => "MetaEvent";
        [EventId]
        public string MetaEventType => "Heartbeat";

        public ServerStatus Status { get; set; }
        public int Interval { get; set; }
    }
}
