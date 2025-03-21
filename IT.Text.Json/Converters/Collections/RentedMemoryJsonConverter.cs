using IT.Text.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Converters;

public class RentedMemoryJsonConverter<T> : JsonConverter<Memory<T>>
{
    private readonly JsonConverter<Memory<T>> _arrayConverter;
    private readonly JsonConverter<T> _itemConverter;
    private readonly int _maxLength;

    public RentedMemoryJsonConverter(JsonSerializerOptions options, int maxLength)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

        _arrayConverter = (JsonConverter<Memory<T>>)options.GetConverter(typeof(Memory<T>));
        _itemConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
        _maxLength = maxLength;
    }

    public override bool HandleNull => true;

    public override Memory<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetRentedArraySegment(_itemConverter, options, _maxLength);
    }

    public override void Write(Utf8JsonWriter writer, Memory<T> value, JsonSerializerOptions options)
    {
        _arrayConverter.Write(writer, value, options);
    }
}