using IT.Text.Json.Extensions;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class MemoryOwnerByteJsonConverter : JsonConverter<IMemoryOwner<byte>?>
{
    private readonly int _maxEncodedLength;
    private readonly byte _rawToken;

    public MemoryOwnerByteJsonConverter(int maxEncodedLength, byte rawToken)
    {
        _maxEncodedLength = maxEncodedLength;
        _rawToken = rawToken;
    }

    public MemoryOwnerByteJsonConverter() : this(int.MaxValue, 0) { }

    public override IMemoryOwner<byte>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetMemoryOwnerFromBase64(_maxEncodedLength, _rawToken);
    }

    public override void Write(Utf8JsonWriter writer, IMemoryOwner<byte>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteBase64(value.Memory.Span);
        }
    }
}