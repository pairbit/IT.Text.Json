using IT.Json.Internal;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Converters;

public class RentedArraySegmentJsonConverter<T> : JsonConverter<ArraySegment<T>>
{
    private static readonly Type _itemType = typeof(T);

    private readonly JsonConverter<Memory<T>> _arrayConverter;
    private readonly JsonConverter<T> _itemConverter;

    public RentedArraySegmentJsonConverter(JsonSerializerOptions options)
    {
        _arrayConverter = (JsonConverter<Memory<T>>)options.GetConverter(typeof(Memory<T>));
        _itemConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
    }

    public override ArraySegment<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected StartArray");

        var itemConverter = _itemConverter;
        T?[] buffer = [];
        var count = 0;
        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    if (buffer.Length == 0) return ArraySegment<T>.Empty;

                    var rented = ArrayPoolShared.Rent<T>(buffer.Length);

                    buffer.AsSpan(0, count).CopyTo(rented);

                    return new ArraySegment<T>(rented, 0, count);
                }

                var item = itemConverter.Read(ref reader, _itemType, options);

                if (buffer.Length == count)
                {
                    var oldBuffer = buffer;

                    buffer = ArrayPool<T>.Shared.Rent(buffer.Length + 1);

                    if (oldBuffer.Length > 0)
                    {
                        oldBuffer.AsSpan().CopyTo(buffer);

                        ArrayPool<T?>.Shared.Return(oldBuffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                        
                        oldBuffer = null;
                    }
                }

                buffer[count++] = item;
            }
        }
        finally
        {
            if (buffer.Length > 0)
                ArrayPool<T?>.Shared.Return(buffer, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }

        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ArraySegment<T> value, JsonSerializerOptions options)
    {
        _arrayConverter.Write(writer, value, options);
    }
}