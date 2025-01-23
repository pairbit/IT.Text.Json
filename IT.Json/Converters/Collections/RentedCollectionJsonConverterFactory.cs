using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedCollectionJsonConverterFactoryAttribute : JsonConverterAttribute
{
    private readonly int _maxLength;

    public RentedCollectionJsonConverterFactoryAttribute() :
        base(typeof(RentedCollectionJsonConverterFactory))
    {
    }

    public RentedCollectionJsonConverterFactoryAttribute(int maxLength)
    {
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

        _maxLength = maxLength;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        if (!RentedCollectionJsonConverterFactory.CheckType(typeToConvert))
            throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");

        return new RentedCollectionJsonConverterFactory(_maxLength);
    }
}

public class RentedCollectionJsonConverterFactory : JsonConverterFactory
{
    private readonly int _maxLength;

    public RentedCollectionJsonConverterFactory()
    {
        _maxLength = int.MaxValue;
    }

    public RentedCollectionJsonConverterFactory(int maxLength)
    {
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));
        _maxLength = maxLength;
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
                    options, _maxLength);
            }
            if (typeDefinition == typeof(Memory<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedMemoryJsonConverter<>).MakeGenericType(arguments[0]),
                    options, _maxLength);
            }
            if (typeDefinition == typeof(ReadOnlyMemory<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedReadOnlyMemoryJsonConverter<>).MakeGenericType(arguments[0]),
                    options, _maxLength);
            }
            if (typeDefinition == typeof(ReadOnlySequence<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(RentedReadOnlySequenceJsonConverter<>).MakeGenericType(arguments[0]),
                    options, _maxLength);
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
}