using IT.Text.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class ReadOnlyMemoryByteJsonConverter : JsonConverter<ReadOnlyMemory<byte>>
{
    private readonly int _maxEncodedLength;
    private readonly byte _rawToken;

    public ReadOnlyMemoryByteJsonConverter(int maxEncodedLength, byte rawToken)
    {
        _maxEncodedLength = maxEncodedLength;
        _rawToken = rawToken;
    }

    public ReadOnlyMemoryByteJsonConverter() : this(int.MaxValue, 0) { }

    public override bool HandleNull => true;

    public override ReadOnlyMemory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetArraySegmentFromBase64(_maxEncodedLength, _rawToken);
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.Span);
    }
}