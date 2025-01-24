using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IT.Json.Internal;

internal static class RentedListShared
{
    [ThreadStatic]
    private static RentedList? _list;

    public static bool IsEnabled => _list != null && _list.enabled;

    public static T[] Rent<T>(int minimumLength)
    {
        var rented = ArrayPool<T>.Shared.Rent(minimumLength);
        var list = _list;
        if (list != null && list.enabled)
        {
            list.Add(new RentedArray(rented, Return<T>));
        }
        return rented;
    }

    internal static void Enable()
    {
        var list = _list;
        if (list == null)
        {
            list = _list = new RentedList();
        }
        list.enabled = true;
    }

    internal static void ReturnAndClear()
    {
        var list = _list;
        if (list != null)
        {
            if (list.Count > 0)
            {
#if NET6_0_OR_GREATER
                foreach (var rentedArray in System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list))
#else
                foreach (var rentedArray in list)
#endif
                {
                    rentedArray.Return(rentedArray.Array);
                }
                list.Clear();
            }
            list.enabled = false;
        }
    }

    internal static void Clear()
    {
        var list = _list;
        if (list != null)
        {
            if (list.Count > 0)
            {
                list.Clear();
            }
            list.enabled = false;
        }
    }

    private static void Return<T>(Array array)
    {
        ArrayPool<T>.Shared.Return((T[])array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    delegate void ReturnArray(Array array);

    readonly struct RentedArray
    {
        public readonly Array Array;
        public readonly ReturnArray Return;

        public RentedArray(Array array, ReturnArray returnArray)
        {
            Array = array;
            Return = returnArray;
        }
    }

    class RentedList : List<RentedArray>
    {
        public bool enabled;
    }
}