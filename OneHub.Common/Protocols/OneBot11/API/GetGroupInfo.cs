using OneHub.Common.Protocols.OneBot11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetGroupInfo
    {
        public ulong GroupId { get; set; }
        public bool NoCache { get; set; } = false;

        public sealed class Response : GroupInfo
        {
        }
    }
}
