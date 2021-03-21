using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EventIdAttribute : JsonConverterAttribute
    {
        public EventIdAttribute() : base(typeof(JsonOptions.ReadOnlyStringPropertyConverter))
        {
        }
    }
}
