using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class ArraySegmentByteJsonConverter : JsonConverter<ArraySegment<byte>>
{
    private readonly int _maxEncodedLength;

    public ArraySegmentByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public ArraySegmentByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override ArraySegment<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var array = reader.GetArrayFromBase64(_maxEncodedLength);
        return array == null ? default : new ArraySegment<byte>(array);
    }

    public override void Write(Utf8JsonWriter writer, ArraySegment<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.AsSpan());
    }
}