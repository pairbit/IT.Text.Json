using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class ArrayByteJsonConverter : JsonConverter<byte[]?>
{
    private readonly int _maxEncodedLength;

    public ArrayByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public ArrayByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetArrayFromBase64(_maxEncodedLength);
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