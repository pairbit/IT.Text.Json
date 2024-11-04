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
}