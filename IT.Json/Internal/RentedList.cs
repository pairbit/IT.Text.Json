using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Internal;

internal sealed class RentedListClass { }

internal sealed class RentedList : JsonConverter<RentedListClass>
{
    private List<byte[]> _list = new();

    public void Add(byte[] rented)
    {
        _list.Add(rented);
    }

    public void ReturnAndClear()
    {
        var list = _list;
        if (list != null)
        {
            foreach (var rented in list)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
            list.Clear();
        }
    }

    public void Clear()
    {
        var list = _list;
        if (list != null)
        {
            list.Clear();
        }
    }

    public override bool HandleNull => false;

    public override bool CanConvert(Type typeToConvert) => false;

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] RentedListClass value, JsonSerializerOptions options)
        => throw new NotSupportedException();

    public override RentedListClass ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException();

    public override int GetHashCode() => throw new NotSupportedException();

    public override bool Equals(object? obj) => throw new NotSupportedException();

    public override RentedListClass? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, RentedListClass value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}