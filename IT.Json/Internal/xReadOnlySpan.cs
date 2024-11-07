using System.Collections.Generic;
using System;

namespace IT.Json.Internal;

internal static class xReadOnlySpan
{
    public static int IndexOfPart<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, out int length)
        where T : IEquatable<T>?
    {
        if (span.Length == 0) throw new ArgumentException("span is empty", nameof(span));

        var maxLength = value.Length;
        if (maxLength == 0) throw new ArgumentException("value is empty", nameof(value));
        if (maxLength == 1)
        {
            var idx = span.IndexOf(value);
            length = idx == -1 ? 0 : 1;
            return idx;
        }

        var index = -1;
        var len = 0;
        var v = value[0];
        for (int i = 0; i < span.Length; i++)
        {
            var s = span[i];
            if (EqualityComparer<T>.Default.Equals(v, s))
            {
                if (index == -1) index = i;
                if (++len == maxLength)
                {
                    length = len;
                    return index;
                }
                v = value[len];
            }
            else if (len > 0)
            {
                v = value[0];
                if (EqualityComparer<T>.Default.Equals(v, s))
                {
                    index = i;
                    len = 1;
                    v = value[1];
                }
                else
                {
                    index = -1;
                    len = 0;
                }
            }
        }
        length = len;
        return index;
    }
}