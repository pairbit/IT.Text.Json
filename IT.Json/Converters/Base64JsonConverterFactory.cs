using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class Base64JsonConverterFactory : JsonConverterFactory
{
    private readonly int _maxEncodedLength;
    private readonly MemoryPool<byte> _pool;

    public Base64JsonConverterFactory()
    {
        _maxEncodedLength = int.MaxValue;
        _pool = MemoryPool<byte>.Shared;
    }

    public Base64JsonConverterFactory(int maxEncodedLength, MemoryPool<byte>? pool = null)
    {
        _maxEncodedLength = maxEncodedLength;
        _pool = pool ?? MemoryPool<byte>.Shared;
    }

    public override bool CanConvert(Type type) =>
        type == typeof(byte[]) ||
        type == typeof(Memory<byte>) ||
        type == typeof(ReadOnlyMemory<byte>) ||
        //type == typeof(ReadOnlySequence<byte>) ||
        type == typeof(IMemoryOwner<byte>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(byte[])) return new ArrayByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(Memory<byte>)) return new MemoryByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(ReadOnlyMemory<byte>)) return new ReadOnlyMemoryByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(IMemoryOwner<byte>)) return new MemoryOwnerByteJsonConverter(_maxEncodedLength, _pool);

        throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");
    }
}