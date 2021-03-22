using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Objects
{
    [JsonConverter(typeof(Enum32JsonConverter<GroupAddType>))]
    public enum GroupAddType
    {
        Unknown,
        Add,
        Invite,
    }
}
