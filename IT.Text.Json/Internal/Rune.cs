#if !NET
using System.Runtime.CompilerServices;

namespace System.Text;

internal readonly struct Rune
{
    private readonly uint _value;

    public Rune(uint value)
    {
        _value = value;
    }

    public Rune(int value) : this((uint)value) { }

    public bool IsAscii => _value <= 0x7Fu;

    public int Value => (int)_value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEncodeToUtf8(Span<byte> destination, out int bytesWritten)
    {
        // The Rune type fits cleanly into a register, so pass byval rather than byref
        // to avoid stack-spilling the 'this' parameter.
        return TryEncodeToUtf8(this, destination, out bytesWritten);
    }

    private static bool TryEncodeToUtf8(Rune value, Span<byte> destination, out int bytesWritten)
    {
        // The bit patterns below come from the Unicode Standard, Table 3-6.

        if (!destination.IsEmpty)
        {
            if (value.IsAscii)
            {
                destination[0] = (byte)value._value;
                bytesWritten = 1;
                return true;
            }

            if (destination.Length > 1)
            {
                if (value.Value <= 0x7FFu)
                {
                    // Scalar 00000yyy yyxxxxxx -> bytes [ 110yyyyy 10xxxxxx ]
                    destination[0] = (byte)((value._value + (0b110u << 11)) >> 6);
                    destination[1] = (byte)((value._value & 0x3Fu) + 0x80u);
                    bytesWritten = 2;
                    return true;
                }

                if (destination.Length > 2)
                {
                    if (value.Value <= 0xFFFFu)
                    {
                        // Scalar zzzzyyyy yyxxxxxx -> bytes [ 1110zzzz 10yyyyyy 10xxxxxx ]
                        destination[0] = (byte)((value._value + (0b1110 << 16)) >> 12);
                        destination[1] = (byte)(((value._value & (0x3Fu << 6)) >> 6) + 0x80u);
                        destination[2] = (byte)((value._value & 0x3Fu) + 0x80u);
                        bytesWritten = 3;
                        return true;
                    }

                    if (destination.Length > 3)
                    {
                        // Scalar 000uuuuu zzzzyyyy yyxxxxxx -> bytes [ 11110uuu 10uuzzzz 10yyyyyy 10xxxxxx ]
                        destination[0] = (byte)((value._value + (0b11110 << 21)) >> 18);
                        destination[1] = (byte)(((value._value & (0x3Fu << 12)) >> 12) + 0x80u);
                        destination[2] = (byte)(((value._value & (0x3Fu << 6)) >> 6) + 0x80u);
                        destination[3] = (byte)((value._value & 0x3Fu) + 0x80u);
                        bytesWritten = 4;
                        return true;
                    }
                }
            }
        }

        // Destination buffer not large enough

        bytesWritten = default;
        return false;
    }
}
#endif