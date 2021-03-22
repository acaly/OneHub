using OneHub.Common.Protocols.OneX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneBot11.Events
{
    [JsonConverter(typeof(Enum32JsonConverter<LifecycleSubType>))]
    public enum LifecycleSubType
    {
        Unknown = 0,
        Enable = 1,
        Disable = 2,
        Connect = 3,
    }

    [OneBot11Event]
    public sealed class Lifecycle : OneBot11Event
    {
        [EventId]
        public string PostType => "MetaEvent";
        [EventId]
        public string MetaEventType => "Lifecycle";

        public LifecycleSubType SubType { get; set; }
    }
}
