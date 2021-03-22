using OneHub.Common.Protocols.OneBot11.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.API
{
    [OneBot11ApiRequest]
    public sealed class GetGroupHonorInfo
    {
        public ulong GroupId { get; set; }

        //Use const values in GroupHonorTypes.
        public string Type { get; set; }

        public sealed class Response
        {
            public ulong GroupId { get; set; }
            public TalkativeInfo CurrentTalkative { get; set; }
            public List<HonorUserInfo> TalkativeList { get; set; }
            public List<HonorUserInfo> PerformerList { get; set; }
            public List<HonorUserInfo> LegendList { get; set; }
            public List<HonorUserInfo> StrongNewbieList { get; set; }
            public List<HonorUserInfo> EmotionList { get; set; }

            public sealed class TalkativeInfo
            {
                public ulong UserId { get; set; }
                public string Nickname { get; set; }
                public string Avatar { get; set; }
                public int DayCount { get; set; }
            }

            public sealed class HonorUserInfo : UserInfo
            {
                public string Avatar { get; set; }
                public string Description { get; set; }
            }
        }
    }
}
