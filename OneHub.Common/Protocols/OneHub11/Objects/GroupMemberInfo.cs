using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Objects
{
    public sealed class GroupMemberInfo : UserInfo
    {
        public string Role { get; set; } //owner, admin, member
        public string Card { get; set; }
        [Obsolete("use JsonConverter")]
        public DateTime JoinTime { get; set; }
        [Obsolete("use JsonConverter")]
        public DateTime LastSentTime { get; set; }
    }
}
