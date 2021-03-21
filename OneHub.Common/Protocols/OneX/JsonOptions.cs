using OneHub.Common.Connections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        internal sealed class ReadOnlyStringPropertyConverter : JsonConverter<string>
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

        internal sealed class TextMessageSerializer<T> : MessageSerializer<T>
        {
            public TextMessageSerializer() : base(JsonOptions.Options)
            {
            }
        }

        internal sealed class BinaryMessageSerializer<T> : IMessageSerializer<T>
        {
            public void Serialize(MessageBuffer messageBuffer, T obj)
            {
                OneXBinaryDecoder.WriteJsonBinary<T>(messageBuffer, obj, ((IBinaryMixedObject)obj).Stream);
            }

            public T Deserialize(MessageBuffer messageBuffer)
            {
                var stream = new MemoryStream();
                var ret = OneXBinaryDecoder.ReadJsonBinary<T>(messageBuffer, stream, JsonOptions.Options);
                ((IBinaryMixedObject)ret).Stream = stream;
                return ret;
            }
        }
    }
}
