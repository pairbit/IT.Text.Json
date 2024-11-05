using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IT.Json.Converters;

public class FlagsEnumJsonConverter<TEnum, TNumber> : EnumJsonConverter<TEnum>
    where TEnum : unmanaged, Enum
    where TNumber : unmanaged, IBitwiseOperators<TNumber, TNumber, TNumber>, IComparisonOperators<TNumber, TNumber, bool>
{
    private readonly byte[] _sep;
    private readonly int _maxLength;
    //private readonly TNumber _sumNumber;
    private readonly Dictionary<TNumber, byte[]> _numberToUtf8Name;

    public FlagsEnumJsonConverter(JsonNamingPolicy? namingPolicy, byte[]? sep = null) : base(namingPolicy)
    {
        if (typeof(TNumber) != typeof(TEnum).GetEnumUnderlyingType())
            throw new ArgumentException($"UnderlyingType enum '{typeof(TEnum).FullName}' is '{typeof(TEnum).GetEnumUnderlyingType().FullName}'", nameof(TNumber));

        sep ??= ", "u8.ToArray();

        var numberToUtf8Name = new Dictionary<TNumber, byte[]>(_valueToUtf8Name.Count);

        var sumNameLength = 0;
        //TNumber sumNumber = default;

        foreach (var pair in _valueToUtf8Name)
        {
            var key = pair.Key;
            TNumber number = Unsafe.As<TEnum, TNumber>(ref key);
            numberToUtf8Name.Add(number, pair.Value);

            //if (number > maxNumber) maxNumber = number;

            sumNameLength += pair.Value.Length;
        }
        _numberToUtf8Name = numberToUtf8Name;
        _maxLength = sumNameLength + (sep.Length * (_valueToUtf8Name.Count - 1));
        _sep = sep;
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (_valueToUtf8Name.TryGetValue(value, out var utf8Name))
        {
            writer.WriteStringValue(utf8Name);
        }
        else
        {
            TNumber numberValue = Unsafe.As<TEnum, TNumber>(ref value);

            var sep = _sep;
            var length = _maxLength;
            //TODO: PoolRent
            Span<byte> utf8Value = stackalloc byte[length];
            var start = length;

            foreach (var pair in _numberToUtf8Name)
            {
                TNumber numberKey = pair.Key;
                if (numberKey == default) continue;

                if ((numberValue & numberKey) == numberKey)
                {
                    if (start != length)
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