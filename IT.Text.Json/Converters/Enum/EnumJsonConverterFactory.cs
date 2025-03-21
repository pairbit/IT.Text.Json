using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class EnumJsonConverterFactory : JsonConverterFactory
{
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly JavaScriptEncoder? _encoder;
    private readonly long _seed;
#if NET7_0_OR_GREATER
    private readonly byte[]? _sep;
#endif

    public EnumJsonConverterFactory(JsonNamingPolicy? namingPolicy, JavaScriptEncoder? encoder = null, long seed = 0
#if NET7_0_OR_GREATER
        , byte[]? sep = null
#endif
        )
    {
        _namingPolicy = namingPolicy;
        _encoder = encoder;
        _seed = seed;
#if NET7_0_OR_GREATER
        _sep = sep;
#endif
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsEnum) throw new ArgumentOutOfRangeException(nameof(typeToConvert), typeToConvert, "type not supported");

        if (typeToConvert.GetCustomAttribute<FlagsAttribute>() != null)
        {
#if NET7_0_OR_GREATER
            return (JsonConverter?)Activator.CreateInstance(
                typeof(FlagsEnumJsonConverter<,>).MakeGenericType(typeToConvert, typeToConvert.GetEnumUnderlyingType()),
                _namingPolicy, _encoder, _seed, _sep);
#else
            throw new NotImplementedException("FlagsEnumJsonConverter is not implemented");
#endif
        }

        return (JsonConverter?)Activator.CreateInstance(typeof(EnumJsonConverter<>).MakeGenericType(typeToConvert),
            _namingPolicy, _encoder, _seed);
    }
}