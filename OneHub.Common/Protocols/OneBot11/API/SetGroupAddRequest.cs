using OneHub.Common.Protocols.OneBot11.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class SetGroupAddRequest
    {
        public string Flag { get; set; }
        public GroupAddType Type { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use Type")]
        public GroupAddType? SubType
        {
            get => null;
            set => Type = value.Value;
        }

        public bool Approve { get; set; } = true;
        public string Reason { get; set; } = string.Empty;

        public sealed class Response
        {
        }
    }
}
