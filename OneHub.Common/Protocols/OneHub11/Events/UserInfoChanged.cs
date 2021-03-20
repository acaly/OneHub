﻿using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Events
{
    [OneHub11Event]
    public sealed class UserInfoChanged
    {
        [EventId]
        public string EventType => nameof(UserInfoChanged);
    }
}
