using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    public abstract class OneBot11Event
    {
        public ulong Time { get; set; }
        public ulong SelfId { get; set; }
    }
}
