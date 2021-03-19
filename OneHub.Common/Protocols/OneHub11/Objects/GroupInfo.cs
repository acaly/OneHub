using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Objects
{
    public sealed class GroupInfo
    {
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string ChannelId { get; set; }
    }
}
