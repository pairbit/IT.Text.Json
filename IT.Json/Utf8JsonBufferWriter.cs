#if NET8_0_OR_GREATER

using IT.Json.Extensions;
using System;
using System.Buffers;
using System.Text.Json;

namespace IT.Json;

public readonly struct Utf8JsonBufferWriter : IBufferWriter<byte>
{
    private readonly Utf8JsonWriter _writer;

    public Utf8JsonBufferWriter(Utf8JsonWriter writer)
    {
        _writer = writer;
    }

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Advance(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "count < 0");

        var writer = _writer;
        var bytesPending = writer.BytesPending + count;
        var memory = writer.GetMemory();
        if (bytesPending > memory.Length)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"count > {memory.Length}");

        writer.SetBytesPending(bytesPending);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        var writer = _writer;
        ref var memory = ref writer.GetMemory();
        var bytesPending = writer.BytesPending;
        
        if (memory.Length - bytesPending < sizeHint)
        {
            //if (writer.GetStream() != null)
            //writer.Flush();
            writer.Grow(sizeHint);
            bytesPending = writer.BytesPending;
        }

        return memory[bytesPending..];
    }

    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;
}

#endif