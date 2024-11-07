using System.IO.Hashing;

namespace IT.Json.Internal;

internal static class xXxHash3
{
    public static int HashToInt32(this XxHash3 xxh) => unchecked((int)xxh.GetCurrentHashAsUInt64());
}