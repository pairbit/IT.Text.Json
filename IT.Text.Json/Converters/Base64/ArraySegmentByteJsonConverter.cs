using IT.Text.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

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
        return reader.GetArraySegmentFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, ArraySegment<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.AsSpan());
    }
}