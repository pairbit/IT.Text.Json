using IT.Text.Json.Extensions;
using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class RentedReadOnlySequenceJsonConverter<T> : JsonConverter<ReadOnlySequence<T?>>
{
    private readonly JsonConverter<ReadOnlyMemory<T?>> _arrayConverter;
    private readonly JsonConverter<T?> _itemConverter;
    private readonly long _maxLength;
    private readonly int _bufferSize;

    public RentedReadOnlySequenceJsonConverter(JsonSerializerOptions options, long maxLength, int bufferSize)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));
        if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        _arrayConverter = (JsonConverter<ReadOnlyMemory<T?>>)options.GetConverter(typeof(ReadOnlyMemory<T>));
        _itemConverter = (JsonConverter<T?>)options.GetConverter(typeof(T));
        _maxLength = maxLength;
        _bufferSize = bufferSize;
    }

    public override bool HandleNull => true;

    public override ReadOnlySequence<T?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetRentedSequence(_itemConverter, options, _maxLength, _bufferSize);
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