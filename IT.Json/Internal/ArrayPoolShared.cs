using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IT.Json.Internal;

internal static class ArrayPoolShared
{
    [ThreadStatic]
    private static RentedList? _rentedList;

    public static bool IsEnabled => _rentedList != null && _rentedList.enabled;

    public static T[] Rent<T>(int minimumLength)
    {
        var rented = ArrayPool<T>.Shared.Rent(minimumLength);
        var rentedList = _rentedList;
        if (rentedList != null && rentedList.enabled)
        {
            rentedList.Add(new RentedArray(rented, Return<T>));
        }
        return rented;
    }

    internal static void AddToList()
    {
        var rentedList = _rentedList;
        if (rentedList == null)
        {
            rentedList = _rentedList = new RentedList();
        }
        rentedList.enabled = true;
    }

    internal static void ReturnAndClear()
    {
        var rentedList = _rentedList;
        if (rentedList != null)
        {
            if (rentedList.Count > 0)
            {
#if NET6_0_OR_GREATER
                foreach (var rentedArray in System.Runtime.InteropServices.CollectionsMarshal.AsSpan(rentedList))
#else
                foreach (var rentedArray in rentedList)
#endif
                {
                    rentedArray.Return(rentedArray.Array);
                }
                rentedList.Clear();
            }
            rentedList.enabled = false;
        }
    }

    internal static void Clear()
    {
        var rentedList = _rentedList;
        if (rentedList != null)
        {
            if (rentedList.Count > 0)
            {
                rentedList.Clear();
            }
            rentedList.enabled = false;
        }
    }

    private static void Return<T>(Array array)
    {
        ArrayPool<T>.Shared.Return((T[])array, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    readonly struct RentedArray
    {
        public readonly Array Array;
        public readonly Action<Array> Return;

        public RentedArray(Array array, Action<Array> returnToPool)
        {
            Array = array;
            Return = returnToPool;
        }
    }

    class RentedList : List<RentedArray>
    {
        public bool enabled;
    }
}