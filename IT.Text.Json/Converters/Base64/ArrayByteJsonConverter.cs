using IT.Text.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class ArrayByteJsonConverter : JsonConverter<byte[]?>
{
    private readonly int _maxEncodedLength;
    private readonly byte _rawToken;

    public ArrayByteJsonConverter(int maxEncodedLength, byte rawToken)
    {
        _maxEncodedLength = maxEncodedLength;
        _rawToken = rawToken;
    }

    public ArrayByteJsonConverter() : this(int.MaxValue, 0) { }

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetArrayFromBase64(_maxEncodedLength, _rawToken);
    }

    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteBase64(value);
        }
    }
}