using OneHub.Common.Definitions;
using OneHub.Common.Protocols.OneHub11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class SetGroupMemberInfo
    {
        public string GroupId { get; set; }
        public GroupMemberInfo Member { get; set; }

        [OneHub11ApiResponse]
        public sealed class Response
        {
        }
    }
}
