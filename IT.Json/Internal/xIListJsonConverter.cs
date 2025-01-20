using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace IT.Json.Internal;

internal static class xIListJsonConverter
{
    public static bool TryGetRentedList(this IList<JsonConverter> converters,
        [NotNullWhen(true)] out RentedList? list)
    {
        for (int i = 0; i < converters.Count; i++)
        {
            var converter = converters[i];
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