using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class CollectionJsonConverterFactory : JsonConverterFactory
{
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
                    typeof(ArraySegmentJsonConverter<>).MakeGenericType(arguments[0]),
                    options);
            }
            if (typeDefinition == typeof(ReadOnlySequence<>))
            {
                return (JsonConverter?)Activator.CreateInstance(
                    typeof(ReadOnlySequenceJsonConverter<>).MakeGenericType(arguments[0]),
                    options);
            }
        }

        throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "Type not supported");
    }

    public static bool CheckType(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType) return false;

        var typeDefinition = typeToConvert.GetGenericTypeDefinition();

        return typeDefinition == typeof(ArraySegment<>) ||
               typeDefinition == typeof(ReadOnlySequence<>);
    }
}