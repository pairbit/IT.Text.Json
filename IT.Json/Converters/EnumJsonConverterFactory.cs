using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class EnumJsonConverterFactory : JsonConverterFactory
{
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly long _seed;
    private readonly byte[]? _sep;

    public EnumJsonConverterFactory(JsonNamingPolicy? namingPolicy, long seed = 0, byte[]? sep = null)
    {
        _namingPolicy = namingPolicy;
        _seed = seed;
        _sep = sep;
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsEnum) throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "type not supported");

        if (typeToConvert.GetCustomAttribute<FlagsAttribute>() != null)
        {
            return (JsonConverter?)Activator.CreateInstance(
                typeof(FlagsEnumJsonConverter<,>).MakeGenericType(typeToConvert, typeToConvert.GetEnumUnderlyingType()),
                _namingPolicy, _seed, _sep);
        }

        return (JsonConverter?)Activator.CreateInstance(typeof(EnumJsonConverter<>).MakeGenericType(typeToConvert),
            _namingPolicy, _seed);
    }
}