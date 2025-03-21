using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class RentedBase64JsonConverterFactoryAttribute : JsonConverterAttribute
{
    private readonly int _maxEncodedLength;
    private readonly byte _rawToken;

    public RentedBase64JsonConverterFactoryAttribute() :
        base(typeof(RentedBase64JsonConverterFactory))
    {
    }

    public RentedBase64JsonConverterFactoryAttribute(int maxEncodedLength, byte rawToken)
    {
        _maxEncodedLength = maxEncodedLength;
        _rawToken = rawToken;
    }

    public override JsonConverter? CreateConverter(Type type)
    {
        if (!RentedBase64JsonConverterFactory.CheckType(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Type not supported");

        return new RentedBase64JsonConverterFactory(_maxEncodedLength, _rawToken);
    }
}

public class RentedBase64JsonConverterFactory : JsonConverterFactory
{
    private readonly int _maxEncodedLength;
    private readonly byte _rawToken;

    public RentedBase64JsonConverterFactory(int maxEncodedLength, byte rawToken)
    {
        _maxEncodedLength = maxEncodedLength;
        _rawToken = rawToken;
    }

    public RentedBase64JsonConverterFactory() : this(int.MaxValue, 0) { }

    public override bool CanConvert(Type type) => CheckType(type);

    public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions options)
    {
        if (type == typeof(ArraySegment<byte>)) return new RentedArraySegmentByteJsonConverter(_maxEncodedLength, _rawToken);
        if (type == typeof(Memory<byte>)) return new RentedMemoryByteJsonConverter(_maxEncodedLength, _rawToken);
        if (type == typeof(ReadOnlyMemory<byte>)) return new RentedReadOnlyMemoryByteJsonConverter(_maxEncodedLength, _rawToken);

        throw new ArgumentOutOfRangeException(nameof(type), type, "Type not supported");
    }

    public static bool CheckType(Type type) =>
        type == typeof(ArraySegment<byte>) ||
        type == typeof(Memory<byte>) ||
        type == typeof(ReadOnlyMemory<byte>);
}