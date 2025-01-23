using IT.Json.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedReadOnlyMemoryJsonConverter<T> : JsonConverter<ReadOnlyMemory<T>>
{
    private readonly JsonConverter<ReadOnlyMemory<T>> _arrayConverter;
    private readonly JsonConverter<T> _itemConverter;
    private readonly int _maxLength;

    public RentedReadOnlyMemoryJsonConverter(JsonSerializerOptions options, int maxLength)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

        _arrayConverter = (JsonConverter<ReadOnlyMemory<T>>)options.GetConverter(typeof(ReadOnlyMemory<T>));
        _itemConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
        _maxLength = maxLength;
    }

    public override bool HandleNull => true;

    public override ReadOnlyMemory<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetRentedArraySegment(_itemConverter, options, _maxLength);
    }

    public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<T> value, JsonSerializerOptions options)
    {
        _arrayConverter.Write(writer, value, options);
    }
}