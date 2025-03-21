using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class ReadOnlySequenceJsonConverter<T> : JsonConverter<ReadOnlySequence<T>>
{
    private readonly JsonConverter<ReadOnlyMemory<T>> _arrayConverter;
    private readonly JsonConverter<T> _itemConverter;

    public ReadOnlySequenceJsonConverter(JsonSerializerOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        _arrayConverter = (JsonConverter<ReadOnlyMemory<T>>)options.GetConverter(typeof(ReadOnlyMemory<T>));
        _itemConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
    }

    public override ReadOnlySequence<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var memory = _arrayConverter.Read(ref reader, typeof(ReadOnlyMemory<T>), options);

        return new ReadOnlySequence<T>(memory);
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlySequence<T> value, JsonSerializerOptions options)
    {
        if (value.IsSingleSegment)
        {
            _arrayConverter.Write(writer, value.First, options);
        }
        else
        {
            writer.WriteStartArray();
            var itemConverter = _itemConverter;
            var position = value.Start;
            while (value.TryGet(ref position, out var memory))
            {
                var span = memory.Span;
                for (int i = 0; i < span.Length; i++)
                {
                    itemConverter.Write(writer, span[i], options);
                }
            }
            writer.WriteEndArray();
        }
    }
}