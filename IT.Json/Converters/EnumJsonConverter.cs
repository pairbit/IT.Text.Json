using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using XXH = System.IO.Hashing.XxHash32;

namespace IT.Json.Converters;

public class EnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    protected readonly int _seed;
    protected readonly int _maxNameLength;
    protected readonly FrozenDictionary<int, TEnum> _xxhToValue;
    protected readonly FrozenDictionary<TEnum, byte[]> _valueToUtf8Name;

    public EnumJsonConverter(JsonNamingPolicy? namingPolicy, int seed = 0)
    {
        var type = typeof(TEnum);
        var values = Enum.GetValues<TEnum>();
        if (values.Length == 0) throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' cannot be empty", nameof(TEnum));

        var utf8 = Encoding.UTF8;
        var xxhToValue = new Dictionary<int, TEnum>(values.Length);
        var valueToUtf8Name = new Dictionary<TEnum, byte[]>(values.Length);
        var maxNameLength = 0;
        
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
            if (utf8Name.Length > maxNameLength) maxNameLength = utf8Name.Length;

            var xxh = (int)XXH.HashToUInt32(utf8Name, seed);

            if (!xxhToValue.TryAdd(xxh, value))
                throw new ArgumentException($"Enum type '{type.FullName}' has collision between '{name}' and '{xxhToValue[xxh]}'. Increment seed", nameof(seed));

            valueToUtf8Name.Add(value, utf8Name);
        }
        _seed = seed;
        _maxNameLength = maxNameLength;
        _xxhToValue = xxhToValue.ToFrozenDictionary();
        _valueToUtf8Name = valueToUtf8Name.ToFrozenDictionary();

#if DEBUG
        var xxhToValueType = _xxhToValue.GetType().FullName;
        var valueToUtf8NameType = _valueToUtf8Name.GetType().FullName;
        if (values.Length <= 10)
        {
            Debug.Assert(xxhToValueType!.StartsWith("System.Collections.Frozen.SmallValueTypeComparableFrozenDictionary`2"));
            Debug.Assert(valueToUtf8NameType!.StartsWith("System.Collections.Frozen.SmallValueTypeComparableFrozenDictionary`2"));
        }
        else
        {
            Debug.Assert(xxhToValueType!.StartsWith("System.Collections.Frozen.Int32FrozenDictionary`1"));
            if (type == typeof(int))
            {
                Debug.Assert(valueToUtf8NameType!.StartsWith("System.Collections.Frozen.Int32FrozenDictionary`1"));
            }
            else
            {
                Debug.Assert(valueToUtf8NameType!.StartsWith("System.Collections.Frozen.ValueTypeDefaultComparerFrozenDictionary`2"));
            }
        }
#endif
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TEnum);

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw new JsonException("Expected string");

        int xxh;

        if (reader.HasValueSequence)
        {
            var sequence = reader.ValueSequence;
            if (sequence.IsSingleSegment)
            {
                var span = sequence.First.Span;

                if (span.Length > _maxNameLength) throw NotMapped(reader.GetString());

                xxh = (int)XXH.HashToUInt32(span, _seed);
            }
            else
            {
                //TODO: static cache?
                var xxhAlg = new XXH(_seed);
                var position = sequence.Start;
                var length = 0;
                while (sequence.TryGet(ref position, out var memory))
                {
                    length += memory.Length;

                    if (length > _maxNameLength) throw NotMapped(reader.GetString());

                    xxhAlg.Append(memory.Span);

                    if (position.GetObject() == null) break;
                }
                xxh = (int)xxhAlg.GetCurrentHashAsUInt32();
            }
        }
        else
        {
            var span = reader.ValueSpan;

            if (span.Length > _maxNameLength) throw NotMapped(reader.GetString());

            xxh = (int)XXH.HashToUInt32(span, _seed);
        }

        if (_xxhToValue.TryGetValue(xxh, out var value)) return value;

        throw NotMapped(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (!_valueToUtf8Name.TryGetValue(value, out var utf8Name)) throw NotMapped(value.ToString());

        writer.WriteStringValue(utf8Name);
    }

    protected static JsonException NotMapped(string? value) => new($"The JSON enum '{value}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.");
}