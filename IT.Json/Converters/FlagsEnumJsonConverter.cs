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
    private readonly TNumber _maxNumber;
    private readonly (TNumber, byte[])[] _numberUtf8Name;

    public FlagsEnumJsonConverter(JsonNamingPolicy? namingPolicy, byte[]? sep = null) : base(namingPolicy)
    {
        if (typeof(TNumber) != typeof(TEnum).GetEnumUnderlyingType())
            throw new ArgumentException($"UnderlyingType enum '{typeof(TEnum).FullName}' is '{typeof(TEnum).GetEnumUnderlyingType().FullName}'", nameof(TNumber));

        var values = Enum.GetValues<TEnum>();
        if (values.Length <= 1) throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' must contain more than one value", nameof(TEnum));

        sep ??= ", "u8.ToArray();

        var sumNameLength = 0;
        TNumber maxNumber = default;

        var numberUtf8Name = new (TNumber, byte[])[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var key = values[i];
            TNumber number = Unsafe.As<TEnum, TNumber>(ref key);
            maxNumber |= number;

            var utf8Name = _valueToUtf8Name[key];
            sumNameLength += utf8Name.Length;

            numberUtf8Name[i] = (number, utf8Name);
        }
        _maxNumber = maxNumber;
        _numberUtf8Name = numberUtf8Name;
        _maxLength = sumNameLength + (sep.Length * (values.Length - 1));
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

            if (numberValue > _maxNumber) goto NotMapped;

            var sep = _sep;
            var length = _maxLength;
            //TODO: PoolRent
            Span<byte> utf8Value = stackalloc byte[length];
            var start = length;

            var numberUtf8Name = _numberUtf8Name;
            for (var i = numberUtf8Name.Length - 1; i >= 0; i--)
            {
                (TNumber number, utf8Name) = numberUtf8Name[i];
                if (number == default) continue;

                if ((numberValue & number) == number)
                {
                    if (start != length)
                    {
                        start -= sep.Length;
                        sep.CopyTo(utf8Value.Slice(start));
                    }

                    start -= utf8Name.Length;
                    utf8Name.CopyTo(utf8Value.Slice(start));

                    numberValue &= ~number;
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