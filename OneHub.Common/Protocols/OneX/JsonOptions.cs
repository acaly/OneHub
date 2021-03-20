using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    internal static class JsonOptions
    {
        public static readonly JsonSerializerOptions Options = CreateSerializerOptions();

        private static JsonSerializerOptions CreateSerializerOptions() => new()
        {
            PropertyNamingPolicy = new NamingPolicy(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private sealed class NamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                return ConvertString(name);
            }
        }

        public static string ConvertString(string str)
        {
            return Regex.Replace(str, "(.)([A-Z][a-z])", "$1_$2").ToLower();
        }

        internal sealed class StringPropertyConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                //Never read from json (used as readonly property).
                return null;
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, ConvertString(value), options);
            }
        }
    }
}
