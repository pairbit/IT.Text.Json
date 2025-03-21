using System.Text.Json;

namespace IT.Json.Extensions;

public static class xJsonSerializerOptions
{
    public static void Init(this JsonSerializerOptions jso, JsonSerializerOptions options)
    {
        jso.DictionaryKeyPolicy = options.DictionaryKeyPolicy;
        jso.PropertyNamingPolicy = options.PropertyNamingPolicy;
        jso.ReadCommentHandling = options.ReadCommentHandling;
        jso.ReferenceHandler = options.ReferenceHandler;
        var converters = jso.Converters;
        if (converters.Count > 0) converters.Clear();
        foreach (var converter in options.Converters)
        {
            converters.Add(converter);
        }
        jso.Encoder = options.Encoder;
        jso.DefaultIgnoreCondition = options.DefaultIgnoreCondition;
        jso.NumberHandling = options.NumberHandling;
        jso.PreferredObjectCreationHandling = options.PreferredObjectCreationHandling;
        jso.UnknownTypeHandling = options.UnknownTypeHandling;
        jso.UnmappedMemberHandling = options.UnmappedMemberHandling;
        jso.DefaultBufferSize = options.DefaultBufferSize;
        jso.MaxDepth = options.MaxDepth;
        jso.AllowTrailingCommas = options.AllowTrailingCommas;
        //jso.IgnoreNullValues = options.IgnoreNullValues;
        jso.IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties;
        jso.IgnoreReadOnlyFields = options.IgnoreReadOnlyFields;
        jso.IncludeFields = options.IncludeFields;
        jso.PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;
        jso.WriteIndented = options.WriteIndented;
        jso.TypeInfoResolver = options.TypeInfoResolver;
    }

    //public static void Reset(this JsonSerializerOptions jso)
    //{
    //    jso.Init(JsonSerializerOptions.Default);
    //}
}