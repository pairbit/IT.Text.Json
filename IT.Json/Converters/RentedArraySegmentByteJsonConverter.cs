using IT.Json.Extensions;
using IT.Json.Internal;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedArraySegmentByteJsonConverter : JsonConverter<ArraySegment<byte>>
{
    private readonly int _maxEncodedLength;

    public RentedArraySegmentByteJsonConverter()
    {
        _maxEncodedLength = int.MaxValue;
    }

    public RentedArraySegmentByteJsonConverter(int maxEncodedLength)
    {
        _maxEncodedLength = maxEncodedLength;
    }

    public override bool HandleNull => true;

    public override ArraySegment<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arraySegment = reader.GetRentedArraySegmentFromBase64(_maxEncodedLength);

        if (!ArrayPoolShared<byte>.IsEnabled)
        {
            var array = arraySegment.Array;
            if (array != null && array.Length > 0)
            {
                if (options.Converters.TryGetRentedList(out var rentedList))
                {
                    rentedList.Add(array);
                }
            }
        }

        return arraySegment;
    }

    public override void Write(Utf8JsonWriter writer, ArraySegment<byte> value, JsonSerializerOptions options)
    {
        writer.WriteBase64(value.AsSpan());
    }
}