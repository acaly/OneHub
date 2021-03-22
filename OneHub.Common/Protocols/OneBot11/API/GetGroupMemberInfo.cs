using OneHub.Common.Protocols.OneBot11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetGroupMemberInfo
    {
        public ulong GroupId { get; set; }
        public ulong UserId { get; set; }
        public bool NoCache { get; set; } = false;

        public sealed class Response : GroupMemberInfo
        {
        }
    }
}
