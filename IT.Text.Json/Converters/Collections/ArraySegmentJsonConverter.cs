using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class ArraySegmentJsonConverter<T> : JsonConverter<ArraySegment<T>>
{
    private readonly JsonConverter<T[]> _arrayConverter;
    private readonly JsonConverter<ReadOnlyMemory<T>> _memoryConverter;

    public ArraySegmentJsonConverter(JsonSerializerOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        _arrayConverter = (JsonConverter<T[]>)options.GetConverter(typeof(T[]));
        _memoryConverter = (JsonConverter<ReadOnlyMemory<T>>)options.GetConverter(typeof(ReadOnlyMemory<T>));
    }

    public override bool HandleNull => true;

    public override ArraySegment<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var array = _arrayConverter.Read(ref reader, typeof(T[]), options);
        return array == null ? default : new ArraySegment<T>(array);
    }

    public override void Write(Utf8JsonWriter writer, ArraySegment<T> value, JsonSerializerOptions options)
    {
        if (value.Array == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            _memoryConverter.Write(writer, value, options);
        }
    }
}