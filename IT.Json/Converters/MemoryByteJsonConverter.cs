using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

internal class MemoryByteJsonConverter : JsonConverter<Memory<byte>>
{
    public override bool HandleNull => true;

    public override Memory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tokenType = reader.TokenType;
        if (tokenType == JsonTokenType.Null) return default;
        if (tokenType != JsonTokenType.String) throw new JsonException("Expected string");

        if (reader.ValueIsEscaped)
        {
            var length = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;
            if (length == 0) return default;

            Memory<byte> memory = new byte[length];

            var span = memory.Span;

            var written = reader.CopyString(span);

            var status = Base64.DecodeFromUtf8InPlace(span.Slice(0, written), out written);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == System.Buffers.OperationStatus.InvalidData)
                    throw new FormatException("Not Base64");

                throw new InvalidOperationException($"OperationStatus is {status}");
            }

            return memory.Slice(0, written);
        }
        else if (reader.HasValueSequence)
        {
            var seq = reader.ValueSequence;
            Memory<byte> decoded = new byte[(seq.Length >> 2) * 3];

            //Base64.DecodeFromUtf8()
            throw new NotImplementedException();
        }
        else
        {
            var utf8 = reader.ValueSpan;
            Memory<byte> decoded = new byte[(utf8.Length >> 2) * 3];

            var status = Base64.DecodeFromUtf8(utf8, decoded.Span, out var consumed, out var written);
            if (status != System.Buffers.OperationStatus.Done)
            {
                if (status == System.Buffers.OperationStatus.InvalidData)
                    throw new FormatException("Not Base64");

                throw new InvalidOperationException($"OperationStatus is {status}");
            }
#if DEBUG
            System.Diagnostics.Debug.Assert(consumed == utf8.Length);
            System.Diagnostics.Debug.Assert(written == decoded.Length);
#endif
            return decoded.Slice(0, written);
        }
    }

    public override void Write(Utf8JsonWriter writer, Memory<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64StringValue(value.Span);
    }
}