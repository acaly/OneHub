using OneHub.Common.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.API
{
    [OneHub11ApiRequest]
    public sealed class GetGroupMemberList
    {
        public string GroupId { get; set; }

        public sealed class Response
        {
            public List<string> Members { get; set; }
        }
    }
}
