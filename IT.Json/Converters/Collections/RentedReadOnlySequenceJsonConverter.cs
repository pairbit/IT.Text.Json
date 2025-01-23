using IT.Json.Extensions;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedReadOnlySequenceJsonConverter<T> : JsonConverter<ReadOnlySequence<T?>>
{
    private readonly JsonConverter<ReadOnlyMemory<T?>> _arrayConverter;
    private readonly JsonConverter<T?> _itemConverter;
    private readonly int _maxLength;

    public RentedReadOnlySequenceJsonConverter(JsonSerializerOptions options, int maxLength)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

        _arrayConverter = (JsonConverter<ReadOnlyMemory<T?>>)options.GetConverter(typeof(ReadOnlyMemory<T>));
        _itemConverter = (JsonConverter<T?>)options.GetConverter(typeof(T));
        _maxLength = maxLength;
    }

    public override bool HandleNull => true;

    public override ReadOnlySequence<T?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetRentedSequence(_itemConverter, options, _maxLength);
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlySequence<T?> value, JsonSerializerOptions options)
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