using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class Base64JsonConverterFactory : JsonConverterFactory
{
    private readonly int _maxEncodedLength;
    private readonly byte _rawToken;

    public Base64JsonConverterFactory(int maxEncodedLength, byte rawToken)
    {
        _maxEncodedLength = maxEncodedLength;
        _rawToken = rawToken;
    }

    public Base64JsonConverterFactory() : this(int.MaxValue, 0) { }

    public override bool CanConvert(Type type) =>
        type == typeof(byte[]) ||
        type == typeof(ArraySegment<byte>) ||
        type == typeof(Memory<byte>) ||
        type == typeof(ReadOnlyMemory<byte>) ||
        //type == typeof(ReadOnlySequence<byte>) ||
        type == typeof(IMemoryOwner<byte>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(byte[])) return new ArrayByteJsonConverter(_maxEncodedLength, _rawToken);
        if (typeToConvert == typeof(ArraySegment<byte>)) return new ArraySegmentByteJsonConverter(_maxEncodedLength, _rawToken);
        if (typeToConvert == typeof(Memory<byte>)) return new MemoryByteJsonConverter(_maxEncodedLength, _rawToken);
        if (typeToConvert == typeof(ReadOnlyMemory<byte>)) return new ReadOnlyMemoryByteJsonConverter(_maxEncodedLength, _rawToken);
        if (typeToConvert == typeof(IMemoryOwner<byte>)) return new MemoryOwnerByteJsonConverter(_maxEncodedLength, _rawToken);

        throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");
    }
}