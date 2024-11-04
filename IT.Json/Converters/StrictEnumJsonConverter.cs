using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using XXH = System.IO.Hashing.XxHash3;

namespace IT.Json.Converters;

public class StrictEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private readonly int _maxLength;
    private readonly Dictionary<ulong, TEnum> _xxhToValue;
    private readonly Dictionary<TEnum, byte[]> _valueToUtf8Name;

    public StrictEnumJsonConverter(JsonNamingPolicy? namingPolicy)
    {
        var type = typeof(TEnum);
        var values = Enum.GetValues<TEnum>();
        var utf8 = Encoding.UTF8;
        var xxhToValue = new Dictionary<ulong, TEnum>();
        var valueToUtf8Name = new Dictionary<TEnum, byte[]>();
        var maxLength = 0;

        foreach (var value in values)
        {
            var name = value.ToString();
            var member = type.GetMember(name)[0];

            var attr = member.GetCustomAttribute<JsonPropertyNameAttribute>(inherit: false);
            if (attr != null)
                name = attr.Name;
            else if (namingPolicy != null)
                name = namingPolicy.ConvertName(name);

            var utf8Name = utf8.GetBytes(name);
            if (utf8Name.Length > maxLength) maxLength = utf8Name.Length;

            var xxh = XXH.HashToUInt64(utf8Name);

            if (!xxhToValue.TryAdd(xxh, value))
                throw new ArgumentException($"Enum type '{type.FullName}' has collision between '{name}' and '{xxhToValue[xxh]}'");

            valueToUtf8Name.Add(value, utf8Name);
        }

        _maxLength = maxLength;
        _xxhToValue = xxhToValue;
        _valueToUtf8Name = valueToUtf8Name;
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw new JsonException("Expected string");

        ulong xxh;

        if (reader.HasValueSequence)
        {
            var sequence = reader.ValueSequence;
            if (sequence.IsSingleSegment)
            {
                var span = sequence.First.Span;

                if (span.Length > _maxLength) throw NotMapped(reader.GetString());

                xxh = XXH.HashToUInt64(span);
            }
            else
            {
                var xxhAlg = new XXH();
                var position = sequence.Start;
                var length = 0;
                while (sequence.TryGet(ref position, out var memory))
                {
                    length += memory.Length;

                    if (length > _maxLength) throw NotMapped(reader.GetString());

                    xxhAlg.Append(memory.Span);

                    if (position.GetObject() == null) break;
                }
                xxh = xxhAlg.GetCurrentHashAsUInt64();
            }
        }
        else
        {
            var span = reader.ValueSpan;

            if (span.Length > _maxLength) throw NotMapped(reader.GetString());

            xxh = XXH.HashToUInt64(span);
        }

        if (_xxhToValue.TryGetValue(xxh, out var value)) return value;

        throw NotMapped(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(_valueToUtf8Name[value]);

    private static JsonException NotMapped(string? value) => new($"The JSON enum '{value}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.");
}