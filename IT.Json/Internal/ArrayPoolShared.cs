using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IT.Json.Internal;

internal static class ArrayPoolShared<T>
{
    [ThreadStatic]
    internal static List<T?[]>? _list;
    private static bool _addToList;

    public static bool IsEnabled => _addToList;

    public static void AddToList()
    {
        _addToList = true;
    }

    public static T?[] Rent(int minimumLength)
    {
        var rented = ArrayPool<T?>.Shared.Rent(minimumLength);
        if (_addToList)
        {
            var list = _list;
            if (list == null)
            {
                list = _list = new List<T?[]>();
            }

            list.Add(rented);
        }
        return rented;
    }

    public static void ReturnAndClear()
    {
        var list = _list;
        if (list != null)
        {
            foreach (var rented in list)
            {
                ArrayPool<T?>.Shared.Return(rented, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            }
            list.Clear();
        }
        _addToList = false;
    }

    public static void Clear()
    {
        var list = _list;
        if (list != null)
        {
            list.Clear();
        }
        _addToList = false;
    }
}