using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Objects
{
    [JsonConverter(typeof(Enum32JsonConverter<SenderInfoSex>))]
    public enum SenderInfoSex
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
    }

    public class UserInfo
    {
        public ulong UserId { get; set; }

        public string Nickname { get; set; }
        public SenderInfoSex Sex { get; set; }
        public int? Age { get; set; }
    }

    public class FriendInfo : UserInfo
    {
        public string Remark { get; set; }
    }

    public class GroupMemberInfo : UserInfo
    {
        //This member is not given in the group message event. Deserialization may give zero.
        public ulong GroupId { get; set; }

        public string Card { get; set; }
        public string Area { get; set; }
        public string Level { get; set; }
        public string Role { get; set; }
        public string Title { get; set; }

        //Extened data.
        //In OneBot11 standard, these are only available from GetGroupMemberInfo and similar apis.

        public int? JoinTime { get; set; }
        public int? LastSentTime { get; set; }
        public bool? Unfriendly { get; set; }
        public string TitleExpireTime { get; set; }
        public bool? CardChangeable { get; set; }
    }
}
