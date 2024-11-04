using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IT.Json.Converters;

public class StrictEnumFlagsJsonConverter<TEnum, TNumber> : StrictEnumJsonConverter<TEnum>
    where TEnum : unmanaged, Enum
    where TNumber : unmanaged, IBitwiseOperators<TNumber, TNumber, TNumber>, IEqualityOperators<TNumber, TNumber, bool>
{
    private readonly byte[] _sep;
    private readonly int _maxLength;

    public StrictEnumFlagsJsonConverter(JsonNamingPolicy? namingPolicy, byte[]? sep = null) : base(namingPolicy)
    {
        typeof(TEnum).GetEnumUnderlyingType();
        sep ??= ", "u8.ToArray();

        var sumNameLength = 0;
        foreach (var pair in _valueToUtf8Name)
        {
            sumNameLength += pair.Value.Length;
        }
        _maxLength = sumNameLength + (sep.Length * (_valueToUtf8Name.Count - 1));
        _sep = sep;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var valueToUtf8Name = _valueToUtf8Name;
        if (valueToUtf8Name.TryGetValue(value, out var utf8Name))
        {
            writer.WriteStringValue(utf8Name);
        }
        else
        {
            TNumber numberValue = Unsafe.As<TEnum, TNumber>(ref value);

            var first = true;
            var sep = _sep;

            //TODO: PoolRent
            Span<byte> utf8Value = stackalloc byte[_maxLength];
            var start = _maxLength;

            foreach (var pair in valueToUtf8Name)
            {
                var key = pair.Key;
                TNumber numberKey = Unsafe.As<TEnum, TNumber>(ref key);
                if (numberKey == default) continue;

                if ((numberValue & numberKey) == numberKey)
                {
                    if (first) first = false;
                    else
                    {
                        start -= sep.Length;
                        sep.CopyTo(utf8Value.Slice(start));
                    }

                    utf8Name = pair.Value;
                    start -= utf8Name.Length;
                    utf8Name.CopyTo(utf8Value.Slice(start));

                    numberValue &= ~numberKey;
                    if (numberValue == default) goto Done;
                }
            }

            if (numberValue != default) goto NotMapped;

        Done:
            writer.WriteStringValue(utf8Value.Slice(start));
            return;

        NotMapped:
            throw NotMapped(value.ToString());
        }
    }
}