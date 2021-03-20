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
    internal sealed class OneXMessageSerializer<T> : IMessageSerializer<T>
    {
        public void Serialize(MessageBuffer messageBuffer, T obj)
        {
            if (IBinaryMixedObject.Helper<T>.IsBinaryMixed)
            {
                WriteJsonBinary(messageBuffer, obj, ((IBinaryMixedObject)obj).Stream);
            }
            else
            {
                messageBuffer.WriteJson(obj, JsonOptions.Options);
            }
        }

        public T Deserialize(MessageBuffer messageBuffer)
        {
            if (IBinaryMixedObject.Helper<T>.IsBinaryMixed)
            {
                var stream = new MemoryStream();
                var ret = ReadJsonBinary(messageBuffer, stream, JsonOptions.Options);
                ((IBinaryMixedObject)ret).Stream = stream;
                return ret;
            }
            else
            {
                return messageBuffer.ReadJson<T>(JsonOptions.Options);
            }
        }

        private static void WriteJsonBinary(MessageBuffer messageBuffer, T obj, Stream binary)
        {
            messageBuffer.Clear();
            messageBuffer.IsBinary = true;
            messageBuffer.Data.SetLength(4);
            JsonSerializer.Serialize(messageBuffer.JsonWriter, obj, JsonOptions.Options);

            var jsonLength = (int)messageBuffer.Data.Length - 4;
            var buffer = messageBuffer.Data.GetBuffer();
            MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateReadOnlySpan(ref jsonLength, 1)).CopyTo(buffer);

            binary.CopyTo(messageBuffer.Data);
        }

        private static T ReadJsonBinary(MessageBuffer messageBuffer, Stream stream, JsonSerializerOptions options)
        {
            if (!messageBuffer.IsBinary)
            {
                throw new InvalidOperationException("Cannot read mixed json-binary data.");
            }
            var buffer = messageBuffer.Data.GetBuffer();
            var jsonLength = BitConverter.ToInt32(buffer, 0);
            var ret = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(buffer, 4, jsonLength), options);
            messageBuffer.Data.Position = jsonLength + 4;
            messageBuffer.Data.CopyTo(stream);
            return ret;
        }
    }
}
