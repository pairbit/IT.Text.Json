using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IT.Json.Extensions;

public static class xUtf8JsonReader
{
    private const byte Pad = (byte)'=';

    public static IMemoryOwner<byte>? GetMemoryOwnerFromBase64(this ref Utf8JsonReader reader, MemoryPool<byte> pool)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return null;
        if (tokenType != JsonTokenType.String) throw new JsonException("Expected string");

        if (reader.ValueIsEscaped) throw new NotSupportedException("Base64 escaping is not supported");
        else if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var maxLengthLong = (seq.Length >> 2) * 3;
            if (maxLengthLong > int.MaxValue) throw new JsonException("too long");
            var maxLength = (int)maxLengthLong;
            var owner = pool.Rent(maxLength);
            var decoded = owner.Memory.Span.Slice(0, maxLength);

            DecodeSequence(seq, decoded, out _, out var written);

            return owner.Slice(0, written);
        }
        else
        {
            var utf8 = reader.ValueSpan;
            var maxLength = (utf8.Length >> 2) * 3;
            var owner = pool.Rent(maxLength);
            var decoded = owner.Memory.Span.Slice(0, maxLength);

            DecodeSpan(utf8, decoded, out _, out var written);

            return owner.Slice(0, written);
        }
    }

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

            DecodeSequence(seq, decoded.Span, out _, out var written);

            return decoded.Slice(0, written);
        }
        else
        {
            var utf8 = reader.ValueSpan;
            Memory<byte> decoded = new byte[(utf8.Length >> 2) * 3];

            DecodeSpan(utf8, decoded.Span, out _, out var written);

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
            var length = seq.Length;
            var decoded = new byte[((length >> 2) * 3) - GetPaddingCount(in seq, seq.GetPosition(length - 2))];

            DecodeSequence(seq, decoded, out var consumed, out var written);
#if DEBUG
            System.Diagnostics.Debug.Assert(consumed == length);
            System.Diagnostics.Debug.Assert(written == decoded.Length);
#endif
            return decoded;
        }
        else
        {
            var utf8 = reader.ValueSpan;
            var decoded = new byte[((utf8.Length >> 2) * 3) - GetPaddingCount(utf8)];

            DecodeSpan(utf8, decoded, out _, out var written);
#if DEBUG
            System.Diagnostics.Debug.Assert(written == decoded.Length);
#endif
            return decoded;
        }
    }

    private static void DecodeSequence(ReadOnlySequence<byte> sequence, Span<byte> bytes, out int consumed, out int written)
    {
        consumed = written = 0;
        int remaining = 0, bytesConsumed, bytesWritten;
        var position = sequence.Start;
        Span<byte> tmpBuffer = stackalloc byte[4];
        OperationStatus status;
        while (sequence.TryGet(ref position, out var memory))
        {
            var length = memory.Length;
            if (length == 0) continue;

            var span = memory.Span;

            if (remaining > 0)
            {
                var need = 4 - remaining;
                if (length < need)
                {
                    if (position.GetObject() == null) throw new InvalidOperationException("InvalidData");
                    span.CopyTo(tmpBuffer[remaining..]);
                    remaining += length;
                    continue;
                }

                span[..need].CopyTo(tmpBuffer[remaining..]);
                remaining = 0;

                status = Base64.DecodeFromUtf8(tmpBuffer, bytes, out bytesConsumed, out bytesWritten);
                if (status != OperationStatus.Done) throw new InvalidOperationException(status.ToString());
#if DEBUG
                System.Diagnostics.Debug.Assert(bytesConsumed == tmpBuffer.Length);
                System.Diagnostics.Debug.Assert(0 < bytesWritten && bytesWritten <= 3);
#endif
                consumed += bytesConsumed;
                written += bytesWritten;
                bytes = bytes[bytesWritten..];

                span = span[need..];
                length = span.Length;
                if (length == 0) continue;
            }

            status = Base64.DecodeFromUtf8(span[..(length & ~0x3)], bytes, out bytesConsumed, out bytesWritten);
            if (status != OperationStatus.Done) throw new InvalidOperationException(status.ToString());
            remaining = length - bytesConsumed;
            if (remaining > 0)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(remaining < 4);
#endif
                span[bytesConsumed..].CopyTo(tmpBuffer);
            }
            consumed += bytesConsumed;
            written += bytesWritten;
            bytes = bytes[bytesWritten..];
        }

        if (remaining > 0) throw new InvalidOperationException("InvalidData");
    }

    private static void DecodeSpan(ReadOnlySpan<byte> utf8, Span<byte> bytes, out int consumed, out int written)
    {
        var status = Base64.DecodeFromUtf8(utf8, bytes, out consumed, out written);
        if (status != OperationStatus.Done)
        {
            if (status == OperationStatus.InvalidData)
                throw new FormatException("Not Base64");

            throw new InvalidOperationException($"OperationStatus is {status}");
        }
#if DEBUG
        System.Diagnostics.Debug.Assert(consumed == utf8.Length);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetPaddingCount(ReadOnlySpan<byte> base64)
        => base64[^1] == Pad ? base64[^2] == Pad ? 2 : 1 : 0;

    private static int GetPaddingCount(in ReadOnlySequence<byte> sequence, SequencePosition position)
    {
        while (sequence.TryGet(ref position, out var memory))
        {
            var length = memory.Length;
            if (length == 0) continue;

            var span = memory.Span;
            if (length == 2) return GetPaddingCount(span);
            if (span[0] == Pad) return 2;

            break;
        }

        while (sequence.TryGet(ref position, out var memory))
        {
            var length = memory.Length;
            if (length == 0) continue;

            var span = memory.Span;
            return span[0] == Pad ? 1 : 0;
        }

        throw new InvalidOperationException("GetPaddingCount");
    }

    private static IMemoryOwner<byte> Slice(this IMemoryOwner<byte> memoryOwner, int start, int length)
    {
        if (start == 0 && length == memoryOwner.Memory.Length) return memoryOwner;

        return new MemoryOwnerSlice<byte>(memoryOwner, start, length);
    }

    class MemoryOwnerSlice<T> : IMemoryOwner<T>
    {
        private readonly IMemoryOwner<T> _memoryOwner;
        private readonly Memory<T> _memory;

        public MemoryOwnerSlice(IMemoryOwner<T> memoryOwner, int start, int length)
        {
            _memoryOwner = memoryOwner;
            _memory = memoryOwner.Memory.Slice(start, length);
        }

        public Memory<T> Memory => _memory;

        public void Dispose() => _memoryOwner.Dispose();
    }
}