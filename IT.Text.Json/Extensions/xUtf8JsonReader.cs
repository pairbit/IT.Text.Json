using IT.Buffers;
using IT.Buffers.Extensions;
using IT.Text.Json.Internal;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Extensions;

public static class xUtf8JsonReader
{
    private const byte Pad = (byte)'=';

    public static ReadOnlySequence<T?> GetRentedSequence<T>(this ref Utf8JsonReader reader,
        JsonConverter<T> itemConverter, JsonSerializerOptions options, long maxLength,
        int bufferSize = BufferSize.KB_64)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.StartArray) throw new JsonException("Expected StartArray");

        T?[] buffer = [];
        SequenceSegment<T?>? start = null;
        SequenceSegment<T?> end = null!;
        int count = 0;
        var itemType = typeof(T);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                if (start == null)
                {
                    Debug.Assert(count == 0);
                    return ReadOnlySequence<T?>.Empty;
                }

                Debug.Assert(count > 0);

                return new ReadOnlySequence<T?>(start, 0, end, count);
            }

            if (0 == maxLength--) throw new JsonException("maxLength");

            var item = itemConverter.Read(ref reader, itemType, options);

            if (buffer.Length == count)
            {
                if (start == null)
                {
                    Debug.Assert(count == 0);

                    buffer = RentedListShared.Rent<T>(bufferSize);

                    start = end = SequenceSegment<T?>.Pool.Rent();
                    start.SetMemory(buffer, isRented: true);
                }
                else
                {
                    Debug.Assert(count > 0);

                    buffer = RentedListShared.Rent<T>(count + 1);

                    end = end.AppendRented(buffer, isRented: true);
                    count = 0;
                }
            }

            buffer[count++] = item;
        }

        throw new JsonException("EndArray not found");
    }

    public static ArraySegment<T> GetRentedArraySegment<T>(this ref Utf8JsonReader reader,
        JsonConverter<T> itemConverter, JsonSerializerOptions options, int maxLength, int bufferSize = 16)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.StartArray) throw new JsonException("Expected StartArray");

        T?[] buffer = [];
        var count = 0;
        var itemType = typeof(T);
        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (count == 0) return ArraySegment<T>.Empty;

                    var rented = RentedListShared.Rent<T>(buffer.Length);

                    buffer.AsSpan(0, count).CopyTo(rented);

                    return new ArraySegment<T>(rented, 0, count);
                }

                if (count == maxLength) throw new JsonException($"maxLength {maxLength}");

                var item = itemConverter.Read(ref reader, itemType, options);

                if (buffer.Length == count)
                {
                    if (count == 0)
                    {
                        buffer = ArrayPool<T>.Shared.Rent(bufferSize);

                        Debug.Assert(buffer.Length >= bufferSize);
                    }
                    else
                    {
                        var oldBuffer = buffer;

                        buffer = ArrayPool<T>.Shared.Rent(count + 1);

                        Debug.Assert(buffer.Length > oldBuffer.Length);

                        oldBuffer.AsSpan().CopyTo(buffer);

                        ArrayPool<T?>.Shared.Return(oldBuffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());

                        oldBuffer = null;
                    }
                }

                buffer[count++] = item;
            }
        }
        finally
        {
            if (buffer.Length > 0)
                ArrayPool<T?>.Shared.Return(buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }

        throw new JsonException("EndArray not found");
    }

    public static IMemoryOwner<byte>? GetMemoryOwnerFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength, byte rawToken = 0)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return null;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength == 0) return null;
            if (longLength > maxEncodedLength) throw TooLong();
            var length = checked((int)longLength);

            if (rawToken != 0 && GetFirst(seq) == rawToken)
            {
                length--;
                var raw = MemoryPool<byte>.Shared.Rent(length);
                seq.Slice(1).CopyTo(raw.Memory.Span);
                return raw.Slice(0, length);
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
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
            if (length == 0) return null;
            if (length > maxEncodedLength) throw TooLong();

            if (rawToken != 0 && span[0] == rawToken)
            {
                length--;
                var raw = MemoryPool<byte>.Shared.Rent(length);
                span.Slice(1).CopyTo(raw.Memory.Span);
                return raw.Slice(0, length);
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var maxLength = (length >> 2) * 3;
            var owner = MemoryPool<byte>.Shared.Rent(maxLength);

            DecodeSpan(span, owner.Memory.Span[..maxLength], out _, out var written);

            return owner.Slice(0, written);
        }
    }

    public static ArraySegment<byte> GetRentedArraySegmentFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength, byte rawToken = 0)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength == 0) return ArraySegment<byte>.Empty;
            if (longLength > maxEncodedLength) throw TooLong();
            var length = checked((int)longLength);

            if (rawToken != 0 && GetFirst(seq) == rawToken)
            {
                length--;
                var raw = RentedListShared.Rent<byte>(length);
                seq.Slice(1).CopyTo(raw);
                if (reader.ValueIsEscaped)
                {
                    //Json.TryUnescape(raw.AsSpan(0, length),)
                }
                return new ArraySegment<byte>(raw, 0, length);
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var maxLength = (length >> 2) * 3;
            var rented = RentedListShared.Rent<byte>(maxLength);

            DecodeSequence(seq, rented.AsSpan(0, maxLength), out _, out var written);

            return new ArraySegment<byte>(rented, 0, written);
        }
        else
        {
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length == 0) return ArraySegment<byte>.Empty;
            if (length > maxEncodedLength) throw TooLong();

            if (rawToken != 0 && span[0] == rawToken)
            {
                length--;
                var raw = RentedListShared.Rent<byte>(length);
                span.Slice(1).CopyTo(raw);
                return new ArraySegment<byte>(raw, 0, length);
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var maxLength = (length >> 2) * 3;
            var rented = RentedListShared.Rent<byte>(maxLength);

            DecodeSpan(span, rented.AsSpan(0, maxLength), out _, out var written);

            return new ArraySegment<byte>(rented, 0, written);
        }
    }

    public static ArraySegment<byte> GetArraySegmentFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength, byte rawToken = 0)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength == 0) return ArraySegment<byte>.Empty;
            if (longLength > maxEncodedLength) throw TooLong();
            var length = checked((int)longLength);

            if (rawToken != 0 && GetFirst(seq) == rawToken)
            {
                var raw = new byte[length - 1];
                seq.Slice(1).CopyTo(raw);
                return raw;
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var decoded = new byte[(length >> 2) * 3];

            DecodeSequence(seq, decoded, out _, out var written);

            return new ArraySegment<byte>(decoded, 0, written);
        }
        else
        {
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length == 0) return ArraySegment<byte>.Empty;
            if (length > maxEncodedLength) throw TooLong();

            if (rawToken != 0 && span[0] == rawToken)
            {
                var raw = new byte[length - 1];
                span.Slice(1).CopyTo(raw);
                return raw;
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var decoded = new byte[(length >> 2) * 3];

            DecodeSpan(span, decoded, out _, out var written);

            return new ArraySegment<byte>(decoded, 0, written);
        }
    }

    public static byte[]? GetArrayFromBase64(this ref Utf8JsonReader reader, int maxEncodedLength, byte rawToken = 0)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return null;
        if (tokenType != JsonTokenType.String) throw NotString();
        if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            var longLength = seq.Length;
            if (longLength == 0) return Array.Empty<byte>();
            if (longLength > maxEncodedLength) throw TooLong();
            var length = checked((int)longLength);

            if (rawToken != 0 && GetFirst(seq) == rawToken)
            {
                var raw = new byte[length - 1];
                seq.Slice(1).CopyTo(raw);
                return raw;
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var decoded = new byte[((length >> 2) * 3) - GetPaddingCount(seq, seq.GetPosition(length - 2))];

            DecodeSequence(seq, decoded, out var consumed, out var written);

            Debug.Assert(consumed == length);
            Debug.Assert(written == decoded.Length);

            return decoded;
        }
        else
        {
            var span = reader.ValueSpan;
            var length = span.Length;
            if (length == 0) return Array.Empty<byte>();
            if (length > maxEncodedLength) throw TooLong();

            if (rawToken != 0 && span[0] == rawToken)
            {
                var raw = new byte[length - 1];
                span.Slice(1).CopyTo(raw);
                return raw;
            }
            if (reader.ValueIsEscaped) throw EscapingNotSupported();
            if (length % 4 != 0) throw InvalidLength();

            var decoded = new byte[((length >> 2) * 3) - GetPaddingCount(span)];

            DecodeSpan(span, decoded, out _, out var written);

            Debug.Assert(written == decoded.Length);

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

                Debug.Assert(bytesConsumed == tmpBuffer.Length);
                Debug.Assert(0 < bytesWritten && bytesWritten <= 3);

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
                Debug.Assert(remaining < 4);

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
        Debug.Assert(consumed == utf8.Length);
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

    private static byte GetFirst(in ReadOnlySequence<byte> sequence)
    {
        var position = sequence.Start;
        while (sequence.TryGet(ref position, out var memory))
        {
            var length = memory.Length;
            if (length == 0) continue;

            return memory.Span[0];
        }

        throw new InvalidOperationException("sequence is empty");
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