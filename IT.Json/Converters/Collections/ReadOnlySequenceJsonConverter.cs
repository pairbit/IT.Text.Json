using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class ReadOnlySequenceJsonConverter<T> : JsonConverter<ReadOnlySequence<T>>
{
    public override ReadOnlySequence<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlySequence<T> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}