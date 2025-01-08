using IT.Json.Extensions;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class ArrayByteJsonConverter : JsonConverter<byte[]?>
{
    private readonly int _maxEncodedLength;

    public ArrayByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public ArrayByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetArrayFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteBase64StringValue(value);
        }
    }
}

public class MemoryOwnerByteJsonConverter : JsonConverter<IMemoryOwner<byte>?>
{
    private readonly int _maxEncodedLength;
    private readonly MemoryPool<byte> _pool;

    public MemoryOwnerByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
        _pool = MemoryPool<byte>.Shared;
    }

    public MemoryOwnerByteJsonConverter(int maxEncodedLength, MemoryPool<byte>? pool = null)
    {
        _maxEncodedLength = maxEncodedLength;
        _pool = pool ?? MemoryPool<byte>.Shared;
    }

    public override IMemoryOwner<byte>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetMemoryOwnerFromBase64(_maxEncodedLength, _pool);
    }

    public override void Write(Utf8JsonWriter writer, IMemoryOwner<byte>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteBase64StringValue(value.Memory.Span);
        }
    }
}

public class MemoryByteJsonConverter : JsonConverter<Memory<byte>>
{
    private readonly int _maxEncodedLength;

    public MemoryByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public MemoryByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override Memory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetMemoryFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, Memory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64StringValue(value.Span);
    }
}

public class ReadOnlyMemoryByteJsonConverter : JsonConverter<ReadOnlyMemory<byte>>
{
    private readonly int _maxEncodedLength;

    public ReadOnlyMemoryByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public ReadOnlyMemoryByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override ReadOnlyMemory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetMemoryFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64StringValue(value.Span);
    }
}