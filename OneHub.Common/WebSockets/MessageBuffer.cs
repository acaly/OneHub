using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace OneHub.Common.WebSockets
{
    public sealed class MessageBuffer : IDisposable
    {
        internal AbstractWebSocketConnection Owner { get; }
        public bool IsBinary { get; internal set; }

        //Use default constructor to allow exposing the underlying buffer.
        internal readonly MemoryStream Data;
        private readonly Utf8JsonWriter _jsonWriter;

        private JsonDocument _jsonDocument;
        private bool _hasJsonDocument;

        private Type _jsonObjType;
        private object _jsonObj;

        internal MessageBuffer(AbstractWebSocketConnection owner)
        {
            if (owner is null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
            Owner = owner;
            Data = new();
            _jsonWriter = new Utf8JsonWriter(Data);
        }

        public void Dispose()
        {
            Owner.ReturnMessageBuffer(this);
        }

        public void Clear()
        {
            Data.Seek(0, SeekOrigin.Begin);
            Data.SetLength(0);
            _jsonDocument?.Dispose();
            _jsonDocument = null;
            _hasJsonDocument = false;
            _jsonObjType = null;
            _jsonObj = null;
            IsBinary = false;
        }

        public JsonDocument ToJsonDocument()
        {
            if (_hasJsonDocument)
            {
                return _jsonDocument;
            }

            var buffer = Data.GetBuffer();
            try
            {
                if (IsBinary)
                {
                    //Binary message:
                    //4 byte length of json part
                    //json part
                    //binary part
                    var jsonLength = BitConverter.ToInt32(buffer, 0);
                    _jsonDocument = JsonDocument.Parse(new ReadOnlyMemory<byte>(buffer, 4, jsonLength));
                }
                else
                {
                    _jsonDocument = JsonDocument.Parse(new ReadOnlyMemory<byte>(buffer, 0, (int)Data.Length));
                }
                _hasJsonDocument = true;
            }
            catch
            {
                _hasJsonDocument = false;
                _jsonDocument = null;
            }
            return _jsonDocument;
        }

        public void WriteJson<T>(T obj, JsonSerializerOptions options)
        {
            Clear();
            JsonSerializer.Serialize(_jsonWriter, obj, options);
        }

        //The returned object is cached and may be shared by other copies. Use immutable types if possible.
        public T ReadJson<T>(JsonSerializerOptions options)
        {
            if (IsBinary)
            {
                throw new InvalidOperationException("Cannot read json data.");
            }
            if (_jsonObjType == typeof(T))
            {
                return (T)_jsonObj;
            }
            var buffer = Data.GetBuffer();
            return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(buffer, 0, (int)Data.Length), options);
        }

        public void WriteBinary(Stream stream)
        {
            Clear();
            IsBinary = true;
            stream.CopyTo(Data);
        }

        public void ReadBinary(Stream stream)
        {
            if (!IsBinary)
            {
                throw new InvalidOperationException("Cannot read binary data.");
            }
            Data.CopyTo(stream);
        }

        public void WriteJsonBinary<T>(T obj, Stream binary, JsonSerializerOptions options)
        {
            Clear();
            IsBinary = true;
            Data.SetLength(4);
            JsonSerializer.Serialize(_jsonWriter, obj, options);

            var jsonLength = (int)Data.Length - 4;
            var buffer = Data.GetBuffer();
            MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateReadOnlySpan(ref jsonLength, 1)).CopyTo(buffer);

            binary.CopyTo(Data);
        }

        public T ReadJsonBinary<T>(Stream stream, JsonSerializerOptions options)
        {
            if (!IsBinary)
            {
                throw new InvalidOperationException("Cannot read mixed json-binary data.");
            }
            var buffer = Data.GetBuffer();
            var jsonLength = BitConverter.ToInt32(buffer, 0);
            var ret = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(buffer, 4, jsonLength), options);
            Data.Position = jsonLength + 4;
            Data.CopyTo(stream);
            return ret;
        }

        public void CopyTo(MessageBuffer messageBuffer)
        {
            messageBuffer.Clear();
            Data.CopyTo(messageBuffer.Data);
            messageBuffer._jsonDocument = _jsonDocument;
            messageBuffer._hasJsonDocument = _hasJsonDocument;
            messageBuffer._jsonObj = _jsonObj;
            messageBuffer._jsonObjType = _jsonObjType;
        }
    }
}
