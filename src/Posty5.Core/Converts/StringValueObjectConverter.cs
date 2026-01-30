using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Posty5.Core.Converts
{
    public sealed class StringValueObjectConverter<T> : JsonConverter<T>
      where T : struct
    {
        private static readonly Dictionary<string, T> _map;

        static StringValueObjectConverter ( )
        {
            _map = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(T))
                .Select(f => (T)f.GetValue(null)!)
                .ToDictionary(
                    x => x!.GetType().GetProperty("Value")!.GetValue(x)!.ToString()!,
                    x => x,
                    StringComparer.OrdinalIgnoreCase
                );
        }

        public override T Read (
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (value is null || !_map.TryGetValue(value, out var result))
                throw new JsonException($"Invalid {typeof(T).Name} value: {value}");

            return result;
        }

        public override void Write (
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options)
        {
            var str = typeof(T).GetProperty("Value")!.GetValue(value)!.ToString();
            writer.WriteStringValue(str);
        }
    }
}
