using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneHub11.Objects
{
    [JsonConverter(typeof(Enum32JsonConverter<ChannelAccessibilities>))]
    [Flags]
    public enum ChannelAccessibilities
    {
        None = 0,
        Read = 1,
        Create = 2,
        Update = 4,
        Delete = 8,
    }

    [JsonConverter(typeof(Enum32JsonConverter<ChannelType>))]
    public enum ChannelType
    {
        Private = 1,
        Group = 2,
        Post = 3,
    }

    public sealed class ChannelInfo
    {
        public string ChannelId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; } //blob
        public ChannelType Type { get; set; }
        public ChannelAccessibilities Accessibilities { get; set; }
    }
}
