﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SetGroupAdmin
    {
        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public bool Enabled { get; set; } = true;

        public sealed class Response
        {
        }
    }
}
