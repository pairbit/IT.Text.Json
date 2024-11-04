using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class JsonStrictEnumConverterFactory : JsonConverterFactory
{
    private readonly JsonNamingPolicy? _namingPolicy;

    public JsonStrictEnumConverterFactory(JsonNamingPolicy? namingPolicy)
    {
        _namingPolicy = namingPolicy;
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsEnum) throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "type not supported");

        var type = typeof(JsonStrictEnumConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter?)Activator.CreateInstance(type, _namingPolicy);
    }
}