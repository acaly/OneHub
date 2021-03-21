using OneHub.Common.Connections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    internal sealed class OneXBinaryDecoder : IBinaryMessageDecoder
    {
        public bool TryDecode(MessageBuffer messageBuffer, out JsonDocument document)
        {
            var stream = messageBuffer.RawData;
            if (stream.Length < 8)
            {
                document = null;
                return false;
            }
            var buffer = stream.GetBuffer();
            var magic = BitConverter.ToInt32(buffer, 0);
            var length = BitConverter.ToInt32(buffer, 4);
            if (magic != 0x58454E4F || length + 8 > stream.Length)
            {
                document = null;
                return false;
            }
            document = JsonDocument.Parse(new ReadOnlyMemory<byte>(buffer, 8, length));
            return true;
        }

        internal static void WriteJsonBinary<T>(MessageBuffer messageBuffer, T obj, MemoryStream binary)
        {
            messageBuffer.Clear();
            messageBuffer.IsBinary = true;

            //Write magic.
            int magic = 0x58454E4F;
            messageBuffer.RawData.Write(MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateSpan(ref magic, 1)));
            messageBuffer.RawData.SetLength(8);
            messageBuffer.RawData.Position = 8;

            //Write json.
            JsonSerializer.Serialize(messageBuffer.JsonWriter, obj, JsonOptions.Options);

            //Write json length.
            var jsonLength = (int)messageBuffer.RawData.Length - 8;
            var buffer = messageBuffer.RawData.GetBuffer();
            MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateReadOnlySpan(ref jsonLength, 1))
                .CopyTo(buffer.AsSpan().Slice(4, 4));
            //Note that this does not move the stream pointer.

            //Write binary.
            binary.CopyTo(messageBuffer.RawData);
        }

        internal static T ReadJsonBinary<T>(MessageBuffer messageBuffer, Stream stream, JsonSerializerOptions options)
        {
            if (!messageBuffer.IsBinary)
            {
                throw new InvalidOperationException("Cannot read mixed json-binary data.");
            }
            var buffer = messageBuffer.RawData.GetBuffer();
            var magic = BitConverter.ToInt32(buffer, 0);
            var jsonLength = BitConverter.ToInt32(buffer, 4);
            if (magic != 0x58454E4F || jsonLength + 8 > buffer.Length)
            {
                throw new InvalidOperationException("ONEX binary format error.");
            }
            var ret = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(buffer, 8, jsonLength), options);
            messageBuffer.RawData.Position = jsonLength + 8;
            messageBuffer.RawData.CopyTo(stream);
            return ret;
        }

        //Binary format:
        //==============
        //4-byte magic: "ONEX" (0x58454E4F)
        //4-byte length of the Json part
        //Json part
        //Binary part
        //==============
        //This must be consistent with the implementation in the message serializer.
    }
}
