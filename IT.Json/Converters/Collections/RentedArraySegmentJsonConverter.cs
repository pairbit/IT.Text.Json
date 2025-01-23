using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedArraySegmentJsonConverter<T> : JsonConverter<ArraySegment<T>>
{
    private readonly JsonConverter<Memory<T>> _arrayConverter;
    private readonly JsonConverter<T> _itemConverter;
    private readonly int _maxLength;

    public RentedArraySegmentJsonConverter(JsonSerializerOptions options, int maxLength)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

        _arrayConverter = (JsonConverter<Memory<T>>)options.GetConverter(typeof(Memory<T>));
        _itemConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
        _maxLength = maxLength;
    }

    public override bool HandleNull => true;

    public override ArraySegment<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetRentedArraySegment(_itemConverter, options, _maxLength);
    }

    public override void Write(Utf8JsonWriter writer, ArraySegment<T> value, JsonSerializerOptions options)
    {
        if (value.Array == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            _arrayConverter.Write(writer, value, options);
        }
    }
}