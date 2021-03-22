using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Objects
{
    public class GroupInfo
    {
        public ulong GroupId { get; set; }
        public string GroupName { get; set; }
        public int MemberCount { get; set; }
        public int MaxMemberCount { get; set; }
    }
}
