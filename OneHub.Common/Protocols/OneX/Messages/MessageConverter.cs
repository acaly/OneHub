using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX.Messages
{
    internal sealed class MessageConverter : JsonConverter<Message>
    {
        public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var rawString = reader.GetString();
                List<AbstractMessageSegment> segments = null;
                try
                {
                    segments = ParseCQString(rawString, options);
                }
                catch
                {
                    //Ignore parsing error, because OneBot11 allows using the raw string as message.
                }
                return new()
                {
                    RawSegments = segments,
                    RawString = rawString,
                };
            }
            else
            {
                return new()
                {
                    RawSegments = JsonSerializer.Deserialize<List<AbstractMessageSegment>>(ref reader, options),
                };
            }
        }

        public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.GetSegments(useRawString: true), options);
        }

        private static List<AbstractMessageSegment> ParseCQString(string str, JsonSerializerOptions options)
        {
            using var ms = new MemoryStream();

            string ParseEscapedString(ReadOnlySpan<char> span)
            {
                return span.ToString().Replace("&amp;", "&").Replace("&#91;", "[").Replace("&#93;", "]").Replace("&#44;", ",");
            }

            AbstractMessageSegment ParseCQSegment(ReadOnlySpan<char> span)
            {
                //Build a json string in ms and parse.
                ms.Position = 0;
                ms.SetLength(0);

                var writer = new Utf8JsonWriter(ms, default);
                writer.WriteStartObject();

                if (!span.StartsWith("CQ:"))
                {
                    throw new JsonException();
                }

                var typeNameEnd = span.IndexOf(',');
                if (typeNameEnd == -1)
                {
                    typeNameEnd = span.Length;
                }
                var typeName = span[3..typeNameEnd];
                writer.WriteString("type", typeName);

                writer.WritePropertyName("data");
                writer.WriteStartObject();
                var remaining = span[typeNameEnd..];
                while (remaining.Length > 0)
                {
                    var argNameEnd = remaining.IndexOf('=');
                    if (argNameEnd == -1)
                    {
                        throw new JsonException();
                    }
                    var argName = remaining[1..argNameEnd];
                    remaining = remaining[(argNameEnd + 1)..];
                    var argEnd = remaining.IndexOf(',');
                    if (argEnd == -1)
                    {
                        argEnd = remaining.Length;
                    }
                    var argVal = remaining[..argEnd];
                    remaining = remaining[argEnd..];
                    writer.WriteString(argName, ParseEscapedString(argVal));
                }
                writer.WriteEndObject();

                writer.WriteEndObject();
                writer.Flush();

                var buffer = ms.GetBuffer();
                return JsonSerializer.Deserialize<AbstractMessageSegment>(new ReadOnlySpan<byte>(buffer, 0, (int)ms.Length), options);
            }

            var strSpan = str.AsSpan();
            var ret = new List<AbstractMessageSegment>();
            while (strSpan.Length > 0)
            {
                if (strSpan[0] == '[')
                {
                    var currentCQEnd = strSpan.IndexOf(']');
                    if (currentCQEnd == -1)
                    {
                        throw new JsonException();
                    }
                    ret.Add(ParseCQSegment(strSpan[1..currentCQEnd]));
                    strSpan = strSpan[(currentCQEnd + 1)..];
                }
                else
                {
                    int nextCQ = strSpan.IndexOf('[');
                    if (nextCQ == -1)
                    {
                        nextCQ = strSpan.Length;
                    }
                    ret.Add(new TextMessageSegment { Text = ParseEscapedString(strSpan[..nextCQ]) });
                    strSpan = strSpan[nextCQ..];
                }
            }
            return ret;
        }
    }
}
