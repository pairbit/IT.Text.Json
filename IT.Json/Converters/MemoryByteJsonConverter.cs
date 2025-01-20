using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class MemoryByteJsonConverter : JsonConverter<Memory<byte>>
{
    private readonly int _maxEncodedLength;

    public MemoryByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public MemoryByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override Memory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetMemoryFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, Memory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.Span);
    }
}