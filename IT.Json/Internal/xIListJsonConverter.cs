using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace IT.Json.Internal;

internal static class xIListJsonConverter
{
    public static RentedList GetRentedList(this IList<JsonConverter> converters)
    {
        var count = converters.Count;
        if (count > 0)
        {
            var converter = converters[count - 1];
            if (converter is RentedList rentedList) return rentedList;
        }
        throw new InvalidOperationException();
    }

    public static bool TryGetRentedList(this IList<JsonConverter> converters,
        [NotNullWhen(true)] out RentedList? list)
    {
        var count = converters.Count;
        if (count > 0)
        {
            var converter = converters[count - 1];
            if (converter is RentedList rentedList)
            {
                list = rentedList;
                return true;
            }
        }
        list = null;
        return false;
    }
}