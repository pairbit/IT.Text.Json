using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IT.Json.Extensions;

public static class xUtf8JsonReader
{
    private const byte Pad = (byte)'=';

    public static Memory<byte> GetMemoryFromBase64(this ref Utf8JsonReader reader)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.String) throw new JsonException("Expected string");

        if (reader.ValueIsEscaped) throw new NotSupportedException("Base64 escaping is not supported");
        //{
        //    var length = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;
        //    if (length == 0) return default;

        //    Memory<byte> memory = new byte[length];

        //    var span = memory.Span;

        //    var written = reader.CopyString(span);

        //    var status = Base64.DecodeFromUtf8InPlace(span.Slice(0, written), out written);
        //    if (status != System.Buffers.OperationStatus.Done)
        //    {
        //        if (status == System.Buffers.OperationStatus.InvalidData)
        //            throw new FormatException("Not Base64");

        //        throw new InvalidOperationException($"OperationStatus is {status}");
        //    }

        //    return memory.Slice(0, written);
        //}
        else if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            Memory<byte> decoded = new byte[(seq.Length >> 2) * 3];

            //Base64.DecodeFromUtf8()
            throw new NotImplementedException();
        }
        else
        {
            var utf8 = reader.ValueSpan;
            Memory<byte> decoded = new byte[(utf8.Length >> 2) * 3];

            var status = Base64.DecodeFromUtf8(utf8, decoded.Span, out var consumed, out var written);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == System.Buffers.OperationStatus.InvalidData)
                    throw new FormatException("Not Base64");

                throw new InvalidOperationException($"OperationStatus is {status}");
            }
#if DEBUG
            System.Diagnostics.Debug.Assert(consumed == utf8.Length);
#endif
            return decoded.Slice(0, written);
        }
    }

    public static byte[]? GetArrayFromBase64(this ref Utf8JsonReader reader)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return null;
        if (tokenType != JsonTokenType.String) throw new JsonException("Expected string");

        if (reader.ValueIsEscaped) throw new NotSupportedException("Base64 escaping is not supported");

        else if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;

            //Base64.DecodeFromUtf8()
            throw new NotImplementedException();
        }
        else
        {
            var utf8 = reader.ValueSpan;
            var decoded = new byte[((utf8.Length >> 2) * 3) - GetPaddingCount(utf8)];

            var status = Base64.DecodeFromUtf8(utf8, decoded, out var consumed, out var written);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == System.Buffers.OperationStatus.InvalidData)
                    throw new FormatException("Not Base64");

                throw new InvalidOperationException($"OperationStatus is {status}");
            }
#if DEBUG
            System.Diagnostics.Debug.Assert(consumed == utf8.Length);
            System.Diagnostics.Debug.Assert(written == decoded.Length);
#endif
            return decoded;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetPaddingCount(ReadOnlySpan<byte> base64)
    {
        if (base64[^1] == Pad)
        {
            return base64[^2] == Pad ? 2 : 1;
        }

        return 0;
    }
}