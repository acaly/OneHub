using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneHub.Common.Protocols.OneX
{
    internal sealed class Enum32JsonConverter<T> : JsonConverter<T> where T : unmanaged, Enum
    {
        private static readonly Dictionary<T, string> _valueToStr = new();
        private static readonly Dictionary<string, T> _strToValue = new();
        private static readonly bool _isFlags = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);

        static Enum32JsonConverter()
        {
            foreach (var v in Enum.GetValues(typeof(T)))
            {
                if (Convert.ToInt32(v) == 0)
                {
                    continue;
                }
                var cv = (T)v;
                var name = JsonOptions.ConvertString(v.ToString());
                _valueToStr.Add(cv, name);
                _strToValue.Add(name, cv);
            }
        }

        //This is why we are limited to Enum32.
        private static T Add(T a, T b)
        {
            uint la = 0, lb = 0;
            Unsafe.As<uint, T>(ref la) = a;
            Unsafe.As<uint, T>(ref lb) = b;
            var ret = la + lb;
            return Unsafe.As<uint, T>(ref ret);
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            T ret = default;
            if (_isFlags)
            {
                var list = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                foreach (var str in list)
                {
                    if (!_strToValue.TryGetValue(str, out var val))
                    {
                        throw new JsonException("Unknown enum value " + str);
                    }
                    ret = Add(ret, val);
                }
            }
            else
            {
                var str = JsonSerializer.Deserialize<string>(ref reader, options);
                if (!_strToValue.TryGetValue(str, out var val))
                {
                    throw new JsonException("Unknown enum value " + str);
                }
                ret = val;
            }
            return ret;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (_isFlags)
            {
                var list = new List<string>();
                foreach (var (k, v) in _strToValue)
                {
#pragma warning disable CA2248 //value is generic type and cannot by itself be marked as [Flags]
                    if (value.HasFlag(v))
                    {
                        list.Add(k);
                    }
#pragma warning restore CA2248
                }
                JsonSerializer.Serialize(writer, list, options);
            }
            else
            {
                if (!_valueToStr.TryGetValue(value, out var str))
                {
                    throw new JsonException("Unknown enum value " + value);
                }
                JsonSerializer.Serialize(writer, str, options);
            }
        }
    }
}
