﻿#if NET7_0_OR_GREATER
using IT.Json.Internal;
using System;
using System.Buffers;
using System.Collections.Concurrent;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace IT.Json.Converters;

public class FlagsEnumJsonConverter<TEnum, TNumber> : EnumJsonConverter<TEnum>
    where TEnum : unmanaged, Enum
    where TNumber : unmanaged
#if NET7_0_OR_GREATER
    , IBitwiseOperators<TNumber, TNumber, TNumber>, IComparisonOperators<TNumber, TNumber, bool>
#endif
{
    private static readonly DisabledJavaScriptEncoder _disabledEncoder = new();
    private const int MaxStackallocBytes = 256;

    private readonly JsonEncodedText _sep;
    private readonly int _maxLength;
    private readonly TNumber _maxNumber;
    private readonly (TNumber, JsonEncodedText)[] _numberUtf8Name;
    private readonly
#if NET8_0_OR_GREATER
        FrozenDictionary
#else
        Dictionary
#endif
        <int, TNumber> _xxhToNumber;

    private readonly ConcurrentDictionary<TEnum, JsonEncodedText> _valuesToUtf8Name;

    static FlagsEnumJsonConverter()
    {
        if (typeof(TNumber) != typeof(TEnum).GetEnumUnderlyingType())
            throw new ArgumentException($"UnderlyingType enum '{typeof(TEnum).FullName}' is '{typeof(TEnum).GetEnumUnderlyingType().FullName}'", nameof(TNumber));

        if (_values.Length == 1) throw new ArgumentException($"Enum '{typeof(TEnum).FullName}' must contain more than one value", nameof(TEnum));
    }

    public FlagsEnumJsonConverter(JsonNamingPolicy? namingPolicy, JavaScriptEncoder? encoder = null, long seed = 0, byte[]? sep = null)
        : base(namingPolicy, encoder, seed)
    {
        if (sep == null || sep.Length == 0) sep = ", "u8.ToArray();

        var sepEncoded = JsonEncodedText.Encode(sep, encoder);

        //TODO: возможно определить более эффективный размер??
        var sumNameLength = 0;
        TNumber maxNumber = default;
        var values = _values;
        var valueToUtf8Name = _valueToUtf8Name;
        var numberUtf8Name = new (TNumber, JsonEncodedText)[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var key = values[i];
            TNumber number = Unsafe.As<TEnum, TNumber>(ref key);
            maxNumber |= number;

            var utf8Name = valueToUtf8Name[key];
            sumNameLength += utf8Name.EncodedUtf8Bytes.Length;

            numberUtf8Name[i] = (number, utf8Name);
        }

        var xxhToNumber = new Dictionary<int, TNumber>(values.Length);
        foreach (var pair in _xxhToValue)
        {
            var value = pair.Value;
            xxhToNumber.Add(pair.Key, Unsafe.As<TEnum, TNumber>(ref value));
        }
        _xxhToNumber = xxhToNumber
#if NET8_0_OR_GREATER
            .ToFrozenDictionary()
#endif
            ;
        _maxNumber = maxNumber;
        _numberUtf8Name = numberUtf8Name;
        _valuesToUtf8Name = new(valueToUtf8Name);
        _maxLength = sumNameLength + (sepEncoded.EncodedUtf8Bytes.Length * (values.Length - 1));
        _sep = sepEncoded;
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw NotString();
        if (reader.HasValueSequence)
        {
            var sequence = reader.ValueSequence;
            if (sequence.IsSingleSegment)
            {
                return TryReadSpan(sequence.First.Span, out var value, out var utf8Name) ? value : throw (utf8Name.Length > 0 ? NotMappedUtf8(utf8Name) : NotMapped(reader.GetString()));
            }
            else
            {
                return TryReadSequence(in sequence, out var value, out var utf8Name) ? value : throw NotMappedUtf8(utf8Name);
            }
        }
        else
        {
            return TryReadSpan(reader.ValueSpan, out var value, out var utf8Name) ? value : throw (utf8Name.Length > 0 ? NotMappedUtf8(utf8Name) : NotMapped(reader.GetString()));
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (_valuesToUtf8Name.TryGetValue(value, out var utf8Name))
        {
            writer.WriteStringValue(utf8Name);
        }
        else
        {
            TNumber numberValue = Unsafe.As<TEnum, TNumber>(ref value);

            if (numberValue > _maxNumber) throw NotMapped(value);

            bool status;
            scoped Span<byte> utf8Value;
            var length = _maxLength;

            if (length <= MaxStackallocBytes)
            {
                utf8Value = stackalloc byte[length];
                status = TryWrite(ref utf8Value, ref numberValue);
                if (status)
                {
                    Debug.Assert(numberValue == default);
                    var encoded = JsonEncodedText.Encode(utf8Value, _disabledEncoder);
                    writer.WriteStringValue(encoded);
                    _valuesToUtf8Name.TryAdd(value, encoded);
                }
            }
            else
            {
                var rented = ArrayPool<byte>.Shared.Rent(length);
                utf8Value = rented.AsSpan(0, length);
                try
                {
                    status = TryWrite(ref utf8Value, ref numberValue);
                    if (status)
                    {
                        Debug.Assert(numberValue == default);
                        var encoded = JsonEncodedText.Encode(utf8Value, _disabledEncoder);
                        writer.WriteStringValue(encoded);
                        _valuesToUtf8Name.TryAdd(value, encoded);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }

            if (!status)
            {
                Debug.Assert(numberValue != default);
                throw Unsafe.As<TEnum, TNumber>(ref value) != numberValue ? NotMappedBit(value, numberValue) : NotMapped(value);
            }
        }
    }

    private bool TryWrite(ref Span<byte> utf8Value, ref TNumber numberValue)
    {
        var length = utf8Value.Length;
        var start = length;
        var sep = _sep.EncodedUtf8Bytes;
        var numberUtf8Name = _numberUtf8Name;
        for (var i = numberUtf8Name.Length - 1; i >= 0; i--)
        {
            (var number, var utf8Name) = numberUtf8Name[i];
            if (number == default) continue;

            if ((numberValue & number) == number)
            {
                if (start != length)
                {
                    start -= sep.Length;
                    sep.CopyTo(utf8Value.Slice(start));
                }
                var utf8NameBytes = utf8Name.EncodedUtf8Bytes;
                start -= utf8NameBytes.Length;
                utf8NameBytes.CopyTo(utf8Value.Slice(start));

                numberValue &= ~number;
                if (numberValue == default)
                {
                    utf8Value = utf8Value.Slice(start);
                    return true;
                }
            }
        }
        utf8Value = utf8Value.Slice(start);
        return false;
    }

    private bool TryReadSpan(ReadOnlySpan<byte> span, out TEnum value, out ReadOnlySpan<byte> utf8Name)
    {
        var sep = _sep.EncodedUtf8Bytes;
        var index = span.IndexOf(sep);
        if (index == -1)
        {
            utf8Name = default;
            return TryReadSpan(span, out value);
        }

        TNumber numberValue = default;
        TNumber number;
        var xxhToNumber = _xxhToNumber;
        var seplen = sep.Length;
        var seed = _seed;
        var maxNameLength = _maxNameLength;
        do
        {
            utf8Name = span.Slice(0, index);
            if (utf8Name.Length > maxNameLength || !xxhToNumber.TryGetValue(HashToInt32(utf8Name, seed), out number))
                goto invalid;

            numberValue |= number;

            span = span.Slice(index + seplen);

            index = span.IndexOf(sep);

        } while (index > -1);

        utf8Name = span;

        if (utf8Name.Length > maxNameLength || !xxhToNumber.TryGetValue(HashToInt32(utf8Name, seed), out number))
            goto invalid;

        numberValue |= number;

        value = Unsafe.As<TNumber, TEnum>(ref numberValue);
        utf8Name = default;
        return true;

    invalid:
        value = default;
        return false;
    }

    private bool TryReadSequence(in ReadOnlySequence<byte> sequence, out TEnum value, out ReadOnlySequence<byte> utf8Name)
    {
        var xxhAlg = GetXXH();
        var xxhToNumber = _xxhToNumber;
        ReadOnlySpan<byte> sep = _sep.EncodedUtf8Bytes;
        var seplen = sep.Length;
        var seplenpart = 0;
        TNumber numberValue = default;
        TNumber number;
        var position = sequence.Start;
        long nameStart = 0;
        var nameLength = 0;
        var maxNameLength = _maxNameLength;
        while (sequence.TryGet(ref position, out var memory))
        {
            var spanlen = memory.Length;
            if (spanlen == 0) continue;

            var span = memory.Span;

            if (seplenpart > 0)
            {
                if (seplen - seplenpart >= spanlen)
                {
                    if (span.SequenceEqual(sep.Slice(seplenpart, spanlen)))
                    {
                        seplenpart += spanlen;
                        continue;
                    }

                    nameLength += seplenpart;
                    if (nameLength <= maxNameLength)
                        xxhAlg.Append(sep.Slice(0, seplenpart));
                }
                else if (span.StartsWith(sep.Slice(seplenpart)))
                {
                    if (nameLength > maxNameLength) goto invalid;
                    if (!xxhToNumber.TryGetValue(xxhAlg.HashToInt32(), out number)) goto invalid;
                    xxhAlg.Reset();

                    numberValue |= number;

                    nameStart += nameLength + seplen;
                    nameLength = 0;

                    span = span.Slice(seplen - seplenpart);
                    spanlen = span.Length;
                    if (spanlen == 0)
                    {
                        seplenpart = 0;
                        //if (position.GetObject() == null) break;
                        continue;
                    }
                }
                else
                {
                    nameLength += seplenpart;
                    if (nameLength <= maxNameLength)
                        xxhAlg.Append(sep.Slice(0, seplenpart));
                }
            }
        indexOf:
            var index = span.IndexOfPart(sep, out seplenpart);
            if (index > -1)
            {
                if (seplen != seplenpart)
                {
                    nameLength += index;
                    if (nameLength <= maxNameLength)
                        xxhAlg.Append(span.Slice(0, index));
                    continue;
                }
                else
                {
                    seplenpart = 0;
                }

                nameLength += index;
                if (nameLength > maxNameLength) goto invalid;

                xxhAlg.Append(span.Slice(0, index));

                if (!xxhToNumber.TryGetValue(xxhAlg.HashToInt32(), out number)) goto invalid;
                xxhAlg.Reset();

                numberValue |= number;

                nameStart += nameLength + seplen;

                nameLength = 0;

                span = span.Slice(index + seplen);

                spanlen = span.Length;

                if (spanlen == 0)
                {
                    //if (position.GetObject() == null) break;
                    continue;
                }
                goto indexOf;
            }

            nameLength += spanlen;
            if (nameLength <= maxNameLength)
                xxhAlg.Append(span);

            //if (position.GetObject() == null) break;
        }

        if (seplenpart > 0)
        {
            nameLength += seplenpart;
            if (nameLength > maxNameLength) goto invalid;

            xxhAlg.Append(sep.Slice(0, seplenpart));
        }
        else if (nameLength > maxNameLength) goto invalid;

        if (!xxhToNumber.TryGetValue(xxhAlg.HashToInt32(), out number)) goto invalid;

        numberValue |= number;
        value = Unsafe.As<TNumber, TEnum>(ref numberValue);
        utf8Name = default;
        return true;

    invalid:
        value = default;
        utf8Name = sequence.Slice(nameStart, nameLength);
        return false;
    }

    private static JsonException NotMappedBit(TEnum value, TNumber bit) =>
        new($"The bit {bit} JSON enum '{value}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.");

    private static JsonException NotMappedUtf8(ReadOnlySpan<byte> utf8)
    {
        var utf16 = Encoding.UTF8.GetString(utf8);

        var str = $"The JSON enum '{utf16}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.";

        return new(str);
    }

    private static JsonException NotMappedUtf8(in ReadOnlySequence<byte> utf8)
    {
        var utf16 = Encoding.UTF8.GetString(utf8);

        var str = $"The JSON enum '{utf16}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.";

        return new(str);
    }

    class DisabledJavaScriptEncoder : JavaScriptEncoder
    {
        public override int MaxOutputCharactersPerInputCharacter
            => throw new NotImplementedException();

        public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text) => -1;

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
            => throw new NotImplementedException();

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            => throw new NotImplementedException();

        public override bool WillEncode(int unicodeScalar)
            => throw new NotImplementedException();
    }
}
#endif