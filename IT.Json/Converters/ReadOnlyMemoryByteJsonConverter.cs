using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class ReadOnlyMemoryByteJsonConverter : JsonConverter<ReadOnlyMemory<byte>>
{
    private readonly int _maxEncodedLength;

    public ReadOnlyMemoryByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public ReadOnlyMemoryByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override ReadOnlyMemory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetMemoryFromBase64(_maxEncodedLength);
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.Span);
    }
}