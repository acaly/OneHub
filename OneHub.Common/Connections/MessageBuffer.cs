using OneHub.Common.Connections.WebSockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace OneHub.Common.Connections
{
    public sealed class MessageBuffer : IDisposable
    {
        internal AbstractWebSocketConnection Owner { get; }
        public bool IsBinary { get; internal set; }

        //Use default constructor to allow exposing the underlying buffer.
        public readonly MemoryStream RawData = new();
        internal readonly Utf8JsonWriter JsonWriter;

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
            JsonWriter = new Utf8JsonWriter(RawData);
        }

        public void Dispose()
        {
            Owner.ReturnMessageBuffer(this);
        }

        public void Clear()
        {
            RawData.Seek(0, SeekOrigin.Begin);
            RawData.SetLength(0);
            _jsonDocument?.Dispose();
            _jsonDocument = null;
            _hasJsonDocument = false;
            _jsonObjType = null;
            _jsonObj = null;
            IsBinary = false;
        }

#warning ToJsonDocument should not specify a binary format
        public JsonDocument ToJsonDocument()
        {
            if (_hasJsonDocument)
            {
                return _jsonDocument;
            }

            try
            {
                if (IsBinary)
                {
                    //Can't decode binary message by itself.
                    //The decoders are registered on the connection.
                    _jsonDocument = Owner.TryDecodeBinaryMessage(this);
                }
                else
                {
                    var buffer = RawData.GetBuffer();
                    _jsonDocument = JsonDocument.Parse(new ReadOnlyMemory<byte>(buffer, 0, (int)RawData.Length));
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
            JsonSerializer.Serialize(JsonWriter, obj, options);
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
            var buffer = RawData.GetBuffer();
            return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(buffer, 0, (int)RawData.Length), options);
        }

        public void WriteBinary(Stream stream)
        {
            Clear();
            IsBinary = true;
            stream.CopyTo(RawData);
        }

        public void ReadBinary(Stream stream)
        {
            if (!IsBinary)
            {
                throw new InvalidOperationException("Cannot read binary data.");
            }
            RawData.CopyTo(stream);
        }

        public void CopyTo(MessageBuffer messageBuffer)
        {
            messageBuffer.Clear();
            RawData.CopyTo(messageBuffer.RawData);
            messageBuffer._jsonDocument = _jsonDocument;
            messageBuffer._hasJsonDocument = _hasJsonDocument;
            messageBuffer._jsonObj = _jsonObj;
            messageBuffer._jsonObjType = _jsonObjType;
        }
    }
}
