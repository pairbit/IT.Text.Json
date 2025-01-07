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
            var decoded = new byte[((seq.Length >> 2) * 3) - GetPaddingCount(in seq, length)];

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
        bool isFinalSegment;
        consumed = written = 0;

        do
        {
            isFinalSegment = sequence.IsSingleSegment;
            ReadOnlySpan<byte> firstSpan = sequence.FirstSpan;

            var status = Base64.DecodeFromUtf8(firstSpan, bytes, out var bytesConsumed, out var bytesWritten, isFinalSegment);
            if (status == OperationStatus.DestinationTooSmall) throw new InvalidOperationException("DestinationTooSmall");
            if (status == OperationStatus.InvalidData) throw new InvalidOperationException("InvalidData");

            consumed += bytesConsumed;
            written += bytesWritten;

            sequence = sequence.Slice(bytesConsumed);
            bytes = bytes.Slice(bytesWritten);

            if (status == OperationStatus.NeedMoreData)
            {
                int remaining = firstSpan.Length - bytesConsumed;
#if DEBUG
                System.Diagnostics.Debug.Assert(remaining > 0 && remaining < 4);
#endif
                // If there are less than 4 elements remaining in this span, process them separately
                // For System.IO.Pipelines this code-path won't be hit, as the default sizes for
                // MinimumSegmentSize are a (higher) power of 2, so are multiples of 4, hence
                // for base64 it is valid or invalid data.
                // Here it is kept to be on the safe side, if non-stanard ROS should be processed.
                DecodeSequenceRemaining(
                        ref sequence,
                        bytes,
                        remaining,
                        ref isFinalSegment,
                        out bytesConsumed,
                        out bytesWritten);

                consumed += bytesConsumed;
                written += bytesWritten;

                bytes = bytes.Slice(bytesWritten);
            }
        } while (!isFinalSegment);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DecodeSequenceRemaining(
        ref ReadOnlySequence<byte> base64,
        Span<byte> bytes,
        int remaining,
        ref bool isFinalSegment,
        out int bytesConsumed,
        out int bytesWritten)
    {
        System.Diagnostics.Debug.Assert(!isFinalSegment);

        ReadOnlySpan<byte> firstSpan = base64.FirstSpan;
        Span<byte> tmpBuffer = stackalloc byte[4];
        firstSpan[^remaining..].CopyTo(tmpBuffer);

        int base64Needed = tmpBuffer.Length - remaining;
        Span<byte> tmpBufferRemaining = tmpBuffer[remaining..];
        base64 = base64.Slice(remaining);
        firstSpan = base64.FirstSpan;

        if (firstSpan.Length > base64Needed)
        {
            firstSpan[0..base64Needed].CopyTo(tmpBufferRemaining);
            base64 = base64.Slice(base64Needed);
        }
        else
        {
            firstSpan.CopyTo(tmpBufferRemaining);
            isFinalSegment = true;
            System.Diagnostics.Debug.Assert(tmpBuffer.Length == remaining + firstSpan.Length);
        }

        var status = Base64.DecodeFromUtf8(tmpBuffer, bytes, out bytesConsumed, out bytesWritten, isFinalSegment);
        if (status != OperationStatus.Done) throw new InvalidOperationException(status.ToString());
#if DEBUG
        System.Diagnostics.Debug.Assert(bytesConsumed == tmpBuffer.Length);
        System.Diagnostics.Debug.Assert(0 < bytesWritten && bytesWritten <= 3);
#endif
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetPaddingCount(in ReadOnlySequence<byte> sequence, long length)
    {
        var end = sequence.Slice(length - 2);
        var span = end.FirstSpan;
        if (span.Length == 2) return GetPaddingCount(span);

        if (span.Length == 1)
        {
            if (span[0] == Pad) return 2;

            end = sequence.Slice(length - 1);
            return end.FirstSpan[0] == Pad ? 1 : 0;
        }

        throw new NotImplementedException();
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