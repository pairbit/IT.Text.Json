using IT.Json.Extensions;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

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
#if NET8_0_OR_GREATER
            writer.WriteBase64Chunked(value.Memory.Span);
#else
            writer.WriteBase64StringValue(value.Memory.Span);
#endif
        }
    }
}