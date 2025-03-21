using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class Base64JsonConverterFactory : JsonConverterFactory
{
    private readonly int _maxEncodedLength;

    public Base64JsonConverterFactory()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public Base64JsonConverterFactory(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool CanConvert(Type type) =>
        type == typeof(byte[]) ||
        type == typeof(ArraySegment<byte>) ||
        type == typeof(Memory<byte>) ||
        type == typeof(ReadOnlyMemory<byte>) ||
        //type == typeof(ReadOnlySequence<byte>) ||
        type == typeof(IMemoryOwner<byte>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(byte[])) return new ArrayByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(ArraySegment<byte>)) return new ArraySegmentByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(Memory<byte>)) return new MemoryByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(ReadOnlyMemory<byte>)) return new ReadOnlyMemoryByteJsonConverter(_maxEncodedLength);
        if (typeToConvert == typeof(IMemoryOwner<byte>)) return new MemoryOwnerByteJsonConverter(_maxEncodedLength);

        throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");
    }
}