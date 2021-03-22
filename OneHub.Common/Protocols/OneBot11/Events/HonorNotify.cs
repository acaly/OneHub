using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<HonorNotifyHonorType>))]
    public enum HonorNotifyHonorType
    {
        Unknown = 0,
        Talkative = 1,
        Performer = 2,
        Emotion = 3,
    }

    [OneBot11Event]
    public sealed class HonorNotify
    {
        [EventId]
        public string PostType => "Notice";
        [EventId]
        public string NoticeType => "Notify";
        [EventId]
        public string SubType => "Honor";

        public ulong GroupId { get; set; }
        public HonorNotifyHonorType HonorType { get; set; }
        public ulong UserId { get; set; }
    }
}
