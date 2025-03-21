using IT.Buffers;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class RentedCollectionJsonConverterFactoryAttribute : JsonConverterAttribute
{
    private readonly long _maxLength;
    private readonly int _bufferSize;

    public RentedCollectionJsonConverterFactoryAttribute() :
        base(typeof(RentedCollectionJsonConverterFactory))
    {
    }

    public RentedCollectionJsonConverterFactoryAttribute(long maxLength, int bufferSize = BufferSize.KB_64)
    {
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        _maxLength = maxLength;
        _bufferSize = bufferSize;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        if (!RentedCollectionJsonConverterFactory.CheckType(typeToConvert))
            throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");

        return new RentedCollectionJsonConverterFactory(_maxLength, _bufferSize);
    }
}

public class RentedCollectionJsonConverterFactory : JsonConverterFactory
{
    private readonly long _maxLength;
    private readonly int _bufferSize;

    public RentedCollectionJsonConverterFactory()
    {
        _maxLength = long.MaxValue;
    }

    public RentedCollectionJsonConverterFactory(long maxLength, int bufferSize = BufferSize.KB_64)
    {
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        _maxLength = maxLength;
        _bufferSize = bufferSize;
    }

    public override bool CanConvert(Type typeToConvert) => CheckType(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsGenericType)
        {
            var typeDefinition = typeToConvert.GetGenericTypeDefinition();
            var arguments = typeToConvert.GetGenericArguments();
            if (typeDefinition == typeof(ArraySegment<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedArraySegmentJsonConverter<>).MakeGenericType(arguments[0]),
                    options, GetMaxLength());
            }
            if (typeDefinition == typeof(Memory<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedMemoryJsonConverter<>).MakeGenericType(arguments[0]),
                    options, GetMaxLength());
            }
            if (typeDefinition == typeof(ReadOnlyMemory<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedReadOnlyMemoryJsonConverter<>).MakeGenericType(arguments[0]),
                    options, GetMaxLength());
            }
            if (typeDefinition == typeof(ReadOnlySequence<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedReadOnlySequenceJsonConverter<>).MakeGenericType(arguments[0]),
                    options, _maxLength, _bufferSize);
            }
        }

        throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");
    }

    public static bool CheckType(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;

        var typeDefinition = typeToConvert.GetGenericTypeDefinition();

        return typeDefinition == typeof(ArraySegment<>) ||
               typeDefinition == typeof(Memory<>) ||
               typeDefinition == typeof(ReadOnlyMemory<>) ||
               typeDefinition == typeof(ReadOnlySequence<>);
    }

    private int GetMaxLength()
    {
        if (_maxLength >= int.MaxValue) return int.MaxValue;

        return (int)_maxLength;
    }
}