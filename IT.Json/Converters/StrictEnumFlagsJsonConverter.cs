using System;
using System.Text.Json;

namespace IT.Json.Converters;

public class StrictEnumFlagsJsonConverter<TEnum> : StrictEnumJsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private readonly byte[] _sep = ", "u8.ToArray();

    public StrictEnumFlagsJsonConverter(JsonNamingPolicy? namingPolicy) : base(namingPolicy)
    {

    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (!_valueToUtf8Name.TryGetValue(value, out var utf8Name)) throw NotMapped(value.ToString());

        writer.WriteStringValue(utf8Name);
    }
}