using OneHub.Common.Protocols.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.Messages
{
    internal sealed class MessageSegmentConverter : JsonConverter<AbstractMessageSegment>
    {
        private static readonly Dictionary<string, Type> _knownTypes = new()
        {
            { "text", typeof(TextMessageSegment) },
            { "image", typeof(ImageMessageSegment) },
        };

        public override AbstractMessageSegment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "type")
            {
                throw new JsonException();
            }
            reader.Read();

            var type = reader.GetString();
            if (!_knownTypes.TryGetValue(type, out var segType))
            {
                segType = typeof(Dictionary<string, string>);
            }
            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "data")
            {
                throw new JsonException();
            }
            reader.Read();

            var data = JsonSerializer.Deserialize(ref reader, segType, options);

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }
            reader.Read();

            if (data is Dictionary<string, string> dict)
            {
                return new UnknownMessageSegment
                {
                    Type = type,
                    Data = dict,
                };
            }
            else
            {
                return (AbstractMessageSegment)data;
            }
        }

        public override void Write(Utf8JsonWriter writer, AbstractMessageSegment value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", value.GetSerializedType());
            writer.WritePropertyName("data");
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
            writer.WriteEndObject();
        }
    }
}
