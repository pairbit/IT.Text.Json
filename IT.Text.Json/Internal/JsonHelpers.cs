using System.Runtime.CompilerServices;

namespace IT.Text.Json.Internal;

internal static class JsonHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInRangeInclusive(uint value, uint lowerBound, uint upperBound)
        => (value - lowerBound) <= (upperBound - lowerBound);
}