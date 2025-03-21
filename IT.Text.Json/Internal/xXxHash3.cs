using System;
using System.IO.Hashing;

namespace IT.Text.Json.Internal;

internal static class xXxHash3
{
    [ThreadStatic]
    private static XxHash3? _default;//seed == 0

    public static int HashToInt32(this XxHash3 xxh) => unchecked((int)xxh.GetCurrentHashAsUInt64());

    public static XxHash3 Default
    {
        get
        {
            var xxh = _default;
            if (xxh == null)
            {
                xxh = _default = new XxHash3();
            }
            else
            {
                xxh.Reset();
            }
            return xxh;
        }
    }
}