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
    public sealed class GetFrientList
    {
        [OneHub11ApiResponse]
        public sealed class Response
        {
            public List<UserInfo> Friends { get; set; }
        }
    }
}
