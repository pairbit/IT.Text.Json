using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedMemoryByteJsonConverter : JsonConverter<Memory<byte>>
{
    private readonly int _maxEncodedLength;

    public RentedMemoryByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public RentedMemoryByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override Memory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetRentedArraySegmentFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, Memory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.Span);
    }
}