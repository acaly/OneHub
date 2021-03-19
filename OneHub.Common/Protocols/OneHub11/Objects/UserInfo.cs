using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Objects
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string Nickname { get; set; }
        public string Remark { get; set; }
        public string Avatar { get; set; }
        public string ChannelId { get; set; }
    }
}
