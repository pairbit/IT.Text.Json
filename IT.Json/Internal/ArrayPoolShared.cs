using System;
using System.Buffers;
using System.Collections.Generic;

namespace IT.Json.Internal;

internal static class ArrayPoolByteShared
{
    private static bool _addToList;

    [ThreadStatic]
    internal static List<byte[]>? _list;

    public static bool IsEnabled => _addToList;

    public static void AddToList()
    {
        _addToList = true;
    }

    public static byte[] Rent(int minimumLength)
    {
        var rented = ArrayPool<byte>.Shared.Rent(minimumLength);
        if (_addToList)
        {
            var list = _list;
            if (list == null)
            {
                list = _list = new List<byte[]>();
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
                ArrayPool<byte>.Shared.Return(rented);
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