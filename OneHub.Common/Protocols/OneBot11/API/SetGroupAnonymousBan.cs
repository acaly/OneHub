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
    public sealed class SetGroupAnonymousBan
    {
        public ulong GroupId { get; set; }
        public AnonymousObject Anonymous { get; set; }
        public string AnonymousFlag { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use AnonymousFlag")]
        public string Flag
        {
            get => null; //Don't serialize.
            set => AnonymousFlag = value;
        }
        public int Duration { get; set; } = 30 * 60;

        public sealed class Response
        {
        }
    }
}
