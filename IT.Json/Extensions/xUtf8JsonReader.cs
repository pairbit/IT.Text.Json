using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IT.Json.Extensions;

public static class xUtf8JsonReader
{
    private const byte Pad = (byte)'=';

    public static IMemoryOwner<byte>? GetMemoryOwnerFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return null;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.ValueIsEscaped) throw EscapingNotSupported();

        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength > maxEncodedLength) throw TooLong();

            var length = (int)longLength;
            if (length % 4 != 0) throw InvalidLength();

            var maxLength = (length >> 2) * 3;
            var owner = MemoryPool<byte>.Shared.Rent(maxLength);

            DecodeSequence(seq, owner.Memory.Span[..maxLength], out _, out var written);

            return owner.Slice(0, written);
        }
        else
        {
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length > maxEncodedLength) throw TooLong();
            if (length % 4 != 0) throw InvalidLength();

            var maxLength = (length >> 2) * 3;
            var owner = MemoryPool<byte>.Shared.Rent(maxLength);

            DecodeSpan(span, owner.Memory.Span[..maxLength], out _, out var written);

            return owner.Slice(0, written);
        }
    }

    public static Memory<byte> GetRentedMemoryFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.ValueIsEscaped) throw EscapingNotSupported();

        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength > maxEncodedLength) throw TooLong();

            var length = (int)longLength;
            if (length % 4 != 0) throw InvalidLength();

            var maxLength = (length >> 2) * 3;
            Memory<byte> decoded = ArrayPool<byte>.Shared.Rent(maxLength);

            DecodeSequence(seq, decoded.Span[..maxLength], out _, out var written);

            return decoded[..written];
        }
        else
        {
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length > maxEncodedLength) throw TooLong();
            if (length % 4 != 0) throw InvalidLength();
            
            var maxLength = (length >> 2) * 3;
            Memory<byte> decoded = ArrayPool<byte>.Shared.Rent(maxLength);

            DecodeSpan(span, decoded.Span[..maxLength], out _, out var written);

            return decoded[..written];
        }
    }

    public static Memory<byte> GetMemoryFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.ValueIsEscaped) throw EscapingNotSupported();

        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength > maxEncodedLength) throw TooLong();

            var length = (int)longLength;
            if (length % 4 != 0) throw InvalidLength();

            Memory<byte> decoded = new byte[(length >> 2) * 3];

            DecodeSequence(seq, decoded.Span, out _, out var written);

            return decoded[..written];
        }
        else
        {
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length > maxEncodedLength) throw TooLong();
            if (length % 4 != 0) throw InvalidLength();

            Memory<byte> decoded = new byte[(length >> 2) * 3];

            DecodeSpan(span, decoded.Span, out _, out var written);

            return decoded[..written];
        }
    }

    public static byte[]? GetArrayFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return null;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.ValueIsEscaped) throw EscapingNotSupported();

        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength > maxEncodedLength) throw TooLong();

            var length = (int)longLength;
            if (length % 4 != 0) throw InvalidLength();

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
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length > maxEncodedLength) throw TooLong();
            if (length % 4 != 0) throw InvalidLength();

            var decoded = new byte[((length >> 2) * 3) - GetPaddingCount(span)];

            DecodeSpan(span, decoded, out _, out var written);
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
    private static int GetPaddingCount(ReadOnlySpan<byte> span)
        => span[^1] == Pad ? span[^2] == Pad ? 2 : 1 : 0;

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

    private static JsonException NotString() => new("Expected string");

    private static JsonException TooLong() => new("Base64 too long");

    private static JsonException InvalidLength() => new("Base64 length is invalid");

    private static JsonException EscapingNotSupported() => new("Base64 escaping is not supported");

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