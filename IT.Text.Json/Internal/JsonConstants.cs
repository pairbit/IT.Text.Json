namespace IT.Text.Json.Internal;

internal static class JsonConstants
{
    public const byte OpenBrace = (byte)'{';
    public const byte CloseBrace = (byte)'}';
    public const byte OpenBracket = (byte)'[';
    public const byte CloseBracket = (byte)']';
    public const byte Space = (byte)' ';
    public const byte CarriageReturn = (byte)'\r';
    public const byte LineFeed = (byte)'\n';
    public const byte Tab = (byte)'\t';
    public const byte ListSeparator = (byte)',';
    public const byte KeyValueSeparator = (byte)':';
    public const byte Quote = (byte)'"';
    public const byte BackSlash = (byte)'\\';
    public const byte Slash = (byte)'/';
    public const byte BackSpace = (byte)'\b';
    public const byte FormFeed = (byte)'\f';
    public const byte Asterisk = (byte)'*';
    public const byte Colon = (byte)':';
    public const byte Period = (byte)'.';
    public const byte Plus = (byte)'+';
    public const byte Hyphen = (byte)'-';
    public const byte UtcOffsetToken = (byte)'Z';
    public const byte TimePrefix = (byte)'T';

    // Encoding Helpers
    public const char HighSurrogateStart = '\ud800';
    public const char HighSurrogateEnd = '\udbff';
    public const char LowSurrogateStart = '\udc00';
    public const char LowSurrogateEnd = '\udfff';

    public const int UnicodePlane01StartValue = 0x10000;
    public const int HighSurrogateStartValue = 0xD800;
    public const int HighSurrogateEndValue = 0xDBFF;
    public const int LowSurrogateStartValue = 0xDC00;
    public const int LowSurrogateEndValue = 0xDFFF;
    public const int BitShiftBy10 = 0x400;
}