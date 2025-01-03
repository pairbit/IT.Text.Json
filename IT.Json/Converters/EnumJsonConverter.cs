using IT.Json.Internal;
using System;
using System.Buffers;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;
using System.IO.Hashing;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class EnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    protected static readonly TEnum[] _values;

    private XxHash3? _xxh;//seed != 0
    protected readonly long _seed;
    protected readonly int _maxNameLength;

    protected readonly
#if NET8_0_OR_GREATER
        FrozenDictionary
#else
        Dictionary
#endif
        <int, TEnum> _xxhToValue;

    protected readonly
#if NET8_0_OR_GREATER
        FrozenDictionary
#else
        Dictionary
#endif
        <TEnum, byte[]> _valueToUtf8Name;

    static EnumJsonConverter()
    {
//#if NET6_0_OR_GREATER
//        var values = Enum.GetValues<TEnum>();
//#else
        var array = Enum.GetValues(typeof(TEnum));
        var values = new TEnum[array.Length];
        Array.Copy(array, values, array.Length);
//#endif
        if (values.Length == 0) throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' cannot be empty", nameof(TEnum));

        _values = values;
    }

    public EnumJsonConverter(JsonNamingPolicy? namingPolicy, long seed = 0)
    {
        var type = typeof(TEnum);
        var values = _values;
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

            var xxh = HashToInt32(utf8Name, seed);
            if (!xxhToValue.TryAdd(xxh, value))
                throw new ArgumentException($"Enum type '{type.FullName}' has collision between '{name}' and '{xxhToValue[xxh]}'. Change name or increment seed", nameof(seed));

            valueToUtf8Name.Add(value, utf8Name);
        }
        _seed = seed;
        _maxNameLength = maxNameLength;
        _xxhToValue = xxhToValue
#if NET8_0_OR_GREATER
            .ToFrozenDictionary()
#endif
            ;
        _valueToUtf8Name = valueToUtf8Name
#if NET8_0_OR_GREATER
            .ToFrozenDictionary()
#endif
            ;

#if NET8_0_OR_GREATER && DEBUG
        var xxhToValueType = _xxhToValue.GetType().FullName;
        var valueToUtf8NameType = _valueToUtf8Name.GetType().FullName;
        if (values.Length <= 10)
        {
            System.Diagnostics.Debug.Assert(xxhToValueType!.StartsWith("System.Collections.Frozen.SmallValueTypeComparableFrozenDictionary`2"));
            System.Diagnostics.Debug.Assert(valueToUtf8NameType!.StartsWith("System.Collections.Frozen.SmallValueTypeComparableFrozenDictionary`2"));
        }
        else
        {
            System.Diagnostics.Debug.Assert(xxhToValueType!.StartsWith("System.Collections.Frozen.Int32FrozenDictionary`1"));
            System.Diagnostics.Debug.Assert(valueToUtf8NameType!.StartsWith("System.Collections.Frozen.ValueTypeDefaultComparerFrozenDictionary`2"));
        }
#endif
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(TEnum);

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw NotString();
        if (reader.ValueIsEscaped) throw NotEscaped();
        if (reader.HasValueSequence)
        {
            var sequence = reader.ValueSequence;
            if (sequence.IsSingleSegment)
            {
                return TryReadSpan(sequence.First.Span, out var value) ? value : throw NotMapped(reader.GetString());
            }
            else
            {
                return TryReadSequence(sequence, out var value) ? value : throw NotMapped(reader.GetString());
            }
        }
        else
        {
            return TryReadSpan(reader.ValueSpan, out var value) ? value : throw NotMapped(reader.GetString());
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (!_valueToUtf8Name.TryGetValue(value, out var utf8Name)) throw NotMapped(value);

        writer.WriteStringValue(utf8Name);
    }

    protected bool TryReadSpan(ReadOnlySpan<byte> span, out TEnum value)
    {
        if (span.Length > _maxNameLength)
        {
            value = default;
            return false;
        }

        return _xxhToValue.TryGetValue(HashToInt32(span, _seed), out value);
    }

    protected XxHash3 GetXXH()
    {
        var xxh = _xxh;
        if (xxh == null)
        {
            xxh = _xxh = (_seed != 0 ? new XxHash3(_seed) : xXxHash3.Default);
        }
        else
        {
            xxh.Reset();
        }
        return xxh;
    }

    protected bool TryReadSequence(ReadOnlySequence<byte> sequence, out TEnum value)
    {
        var xxhAlg = GetXXH();
        var position = sequence.Start;
        var length = 0;
        while (sequence.TryGet(ref position, out var memory))
        {
            length += memory.Length;

            if (length > _maxNameLength)
            {
                value = default;
                return false;
            }

            xxhAlg.Append(memory.Span);

            if (position.GetObject() == null) break;
        }

        return _xxhToValue.TryGetValue(xxhAlg.HashToInt32(), out value);
    }

    protected static int HashToInt32(ReadOnlySpan<byte> span, long seed)
        => unchecked((int)XxHash3.HashToUInt64(span, seed));

    protected static JsonException NotEscaped() => new("Escaped value is not supported");

    protected static JsonException NotString() => new("Expected string");

    protected static JsonException NotMapped<T>(T value) =>
        new($"The JSON enum '{value}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.");
}