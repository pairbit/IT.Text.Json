using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedCollectionJsonConverterFactory : JsonConverterFactory
{
    private readonly int _maxLength;

    public RentedCollectionJsonConverterFactory()
    {
        _maxLength = int.MaxValue;
    }

    public RentedCollectionJsonConverterFactory(int maxLength)
    {
        _maxLength = maxLength;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;

        var typeDefinition = typeToConvert.GetGenericTypeDefinition();

        return typeDefinition == typeof(ArraySegment<>);
    }

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
        }

        throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");
    }
}