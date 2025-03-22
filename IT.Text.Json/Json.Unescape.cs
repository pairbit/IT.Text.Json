using IT.Text.Json.Internal;
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Text;

namespace IT.Text.Json;

public static partial class Json
{
    public static ArraySegment<byte> Unescape(ReadOnlySpan<byte> source)
    {
        var destination = new byte[source.Length];

        var result = TryUnescape(source, destination, out var written);
        Debug.Assert(result);

        return new(destination, 0, written);
    }

    /// <exception cref="ArgumentException">destination too small</exception>
    public static void Unescape(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
    {
        if (source.Length > destination.Length) throw new ArgumentException("destination too small", nameof(destination));

        var result = TryUnescape(source, destination, out written);
        Debug.Assert(result);
    }

    public static void UnsafeUnescape(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
    {
        var result = TryUnescape(source, destination, out written);
        Debug.Assert(result);
    }

    public static int GetFirstEscaped(ReadOnlySpan<byte> source)
        => source.IndexOf(JsonConstants.BackSlash);

    public static bool TryUnescape(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
    {
        int idx = source.IndexOf(JsonConstants.BackSlash);
        if (idx >= 0) return TryUnescape(source, destination, idx, out written);

        if (source.TryCopyTo(destination))
        {
            written = source.Length;
            return true;
        }
        written = 0;
        return false;
    }

    public static void UnescapeInPlace(Span<byte> source, out int written)
    {
        int idx = source.IndexOf(JsonConstants.BackSlash);
        if (idx >= 0)
        {
            UnescapeInPlace(source, idx, out written);
        }
        else
        {
            written = source.Length;
        }
    }

    private static void UnescapeInPlace(Span<byte> source, int idx, out int written)
    {
        Debug.Assert(idx >= 0 && idx < source.Length);
        Debug.Assert(source[idx] == JsonConstants.BackSlash);

        written = idx;

        while (true)
        {
            Debug.Assert(source[idx] == JsonConstants.BackSlash);

            switch (source[++idx])
            {
                case JsonConstants.Quote:
                    source[written++] = JsonConstants.Quote;
                    break;
                case (byte)'n':
                    source[written++] = JsonConstants.LineFeed;
                    break;
                case (byte)'r':
                    source[written++] = JsonConstants.CarriageReturn;
                    break;
                case JsonConstants.BackSlash:
                    source[written++] = JsonConstants.BackSlash;
                    break;
                case JsonConstants.Slash:
                    source[written++] = JsonConstants.Slash;
                    break;
                case (byte)'t':
                    source[written++] = JsonConstants.Tab;
                    break;
                case (byte)'b':
                    source[written++] = JsonConstants.BackSpace;
                    break;
                case (byte)'f':
                    source[written++] = JsonConstants.FormFeed;
                    break;
                default:
                    var rune = new Rune(GetScalar(source, ref idx));
                    bool success = rune.TryEncodeToUtf8(source.Slice(written), out int bytesWritten);
                    Debug.Assert(success);
                    Debug.Assert(bytesWritten <= 4);
                    written += bytesWritten;
                    break;
            }

            if (++idx == source.Length) return;

            if (source[idx] != JsonConstants.BackSlash)
            {
                ReadOnlySpan<byte> remaining = source.Slice(idx);
                int nextUnescapedSegmentLength = remaining.IndexOf(JsonConstants.BackSlash);
                if (nextUnescapedSegmentLength < 0)
                {
                    nextUnescapedSegmentLength = remaining.Length;
                }

                Debug.Assert(nextUnescapedSegmentLength > 0);
                switch (nextUnescapedSegmentLength)
                {
                    case 1:
                        source[written++] = source[idx++];
                        break;
                    case 2:
                        source[written++] = source[idx++];
                        source[written++] = source[idx++];
                        break;
                    case 3:
                        source[written++] = source[idx++];
                        source[written++] = source[idx++];
                        source[written++] = source[idx++];
                        break;
                    default:
                        remaining.Slice(0, nextUnescapedSegmentLength).CopyTo(source.Slice(written));
                        written += nextUnescapedSegmentLength;
                        idx += nextUnescapedSegmentLength;
                        break;
                }

                Debug.Assert(idx == source.Length || source[idx] == JsonConstants.BackSlash);

                if (idx == source.Length) return;
            }
        }
    }

    private static bool TryUnescape(ReadOnlySpan<byte> source, Span<byte> destination, int idx, out int written)
    {
        Debug.Assert(idx >= 0 && idx < source.Length);
        Debug.Assert(source[idx] == JsonConstants.BackSlash);

        if (!source.Slice(0, idx).TryCopyTo(destination))
        {
            written = 0;
            goto DestinationTooShort;
        }

        written = idx;

        while (true)
        {
            Debug.Assert(source[idx] == JsonConstants.BackSlash);

            if (written == destination.Length)
            {
                goto DestinationTooShort;
            }

            switch (source[++idx])
            {
                case JsonConstants.Quote:
                    destination[written++] = JsonConstants.Quote;
                    break;
                case (byte)'n':
                    destination[written++] = JsonConstants.LineFeed;
                    break;
                case (byte)'r':
                    destination[written++] = JsonConstants.CarriageReturn;
                    break;
                case JsonConstants.BackSlash:
                    destination[written++] = JsonConstants.BackSlash;
                    break;
                case JsonConstants.Slash:
                    destination[written++] = JsonConstants.Slash;
                    break;
                case (byte)'t':
                    destination[written++] = JsonConstants.Tab;
                    break;
                case (byte)'b':
                    destination[written++] = JsonConstants.BackSpace;
                    break;
                case (byte)'f':
                    destination[written++] = JsonConstants.FormFeed;
                    break;
                default:
                    var rune = new Rune(GetScalar(source, ref idx));
                    bool success = rune.TryEncodeToUtf8(destination.Slice(written), out int bytesWritten);
                    if (!success)
                    {
                        goto DestinationTooShort;
                    }

                    Debug.Assert(bytesWritten <= 4);
                    written += bytesWritten;
                    break;
            }

            if (++idx == source.Length)
            {
                goto Success;
            }

            if (source[idx] != JsonConstants.BackSlash)
            {
                ReadOnlySpan<byte> remaining = source.Slice(idx);
                int nextUnescapedSegmentLength = remaining.IndexOf(JsonConstants.BackSlash);
                if (nextUnescapedSegmentLength < 0)
                {
                    nextUnescapedSegmentLength = remaining.Length;
                }

                if ((uint)(written + nextUnescapedSegmentLength) >= (uint)destination.Length)
                {
                    goto DestinationTooShort;
                }

                Debug.Assert(nextUnescapedSegmentLength > 0);
                switch (nextUnescapedSegmentLength)
                {
                    case 1:
                        destination[written++] = source[idx++];
                        break;
                    case 2:
                        destination[written++] = source[idx++];
                        destination[written++] = source[idx++];
                        break;
                    case 3:
                        destination[written++] = source[idx++];
                        destination[written++] = source[idx++];
                        destination[written++] = source[idx++];
                        break;
                    default:
                        remaining.Slice(0, nextUnescapedSegmentLength).CopyTo(destination.Slice(written));
                        written += nextUnescapedSegmentLength;
                        idx += nextUnescapedSegmentLength;
                        break;
                }

                Debug.Assert(idx == source.Length || source[idx] == JsonConstants.BackSlash);

                if (idx == source.Length)
                {
                    goto Success;
                }
            }
        }

    Success:
        return true;

    DestinationTooShort:
        return false;
    }

    private static int GetScalar(ReadOnlySpan<byte> source, ref int idx)
    {
        Debug.Assert(source[idx] == 'u', "invalid escape sequences must have already been caught by Utf8JsonReader.Read()");

        // The source is known to be valid JSON, and hence if we see a \u, it is guaranteed to have 4 hex digits following it
        // Otherwise, the Utf8JsonReader would have already thrown an exception.
        Debug.Assert(source.Length >= idx + 5);

        bool result = Utf8Parser.TryParse(source.Slice(idx + 1, 4), out int scalar, out int bytesConsumed, 'x');
        Debug.Assert(result);
        Debug.Assert(bytesConsumed == 4);
        idx += 4;

        if (JsonHelpers.IsInRangeInclusive((uint)scalar, JsonConstants.HighSurrogateStartValue, JsonConstants.LowSurrogateEndValue))
        {
            // The first hex value cannot be a low surrogate.
            if (scalar >= JsonConstants.LowSurrogateStartValue)
            {
                throw InvalidUTF16(scalar);
            }

            Debug.Assert(JsonHelpers.IsInRangeInclusive((uint)scalar, JsonConstants.HighSurrogateStartValue, JsonConstants.HighSurrogateEndValue));

            // We must have a low surrogate following a high surrogate.
            if (source.Length < idx + 7 || source[idx + 1] != '\\' || source[idx + 2] != 'u')
            {
                throw IncompleteUTF16();
            }

            // The source is known to be valid JSON, and hence if we see a \u, it is guaranteed to have 4 hex digits following it
            // Otherwise, the Utf8JsonReader would have already thrown an exception.
            result = Utf8Parser.TryParse(source.Slice(idx + 3, 4), out int lowSurrogate, out bytesConsumed, 'x');
            Debug.Assert(result);
            Debug.Assert(bytesConsumed == 4);
            idx += 6;

            // If the first hex value is a high surrogate, the next one must be a low surrogate.
            if (!JsonHelpers.IsInRangeInclusive((uint)lowSurrogate, JsonConstants.LowSurrogateStartValue, JsonConstants.LowSurrogateEndValue))
            {
                throw InvalidUTF16(lowSurrogate);
            }

            // To find the unicode scalar:
            // (0x400 * (High surrogate - 0xD800)) + Low surrogate - 0xDC00 + 0x10000
            scalar = (JsonConstants.BitShiftBy10 * (scalar - JsonConstants.HighSurrogateStartValue))
                + (lowSurrogate - JsonConstants.LowSurrogateStartValue)
                + JsonConstants.UnicodePlane01StartValue;
        }

        return scalar;
    }

    private static InvalidOperationException InvalidUTF16(int charAsInt)
        => new($"Can not read invalid utf16: 0x{charAsInt:X2}");

    private static InvalidOperationException IncompleteUTF16()
        => new("Can not read incomplete utf16");
}