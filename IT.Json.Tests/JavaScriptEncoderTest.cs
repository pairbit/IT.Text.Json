using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace IT.Json.Tests;

public class JavaScriptEncoderTest
{
    class DisableJavaScriptEncoder : JavaScriptEncoder
    {
        public static readonly DisableJavaScriptEncoder Instance = new();

        public override int MaxOutputCharactersPerInputCharacter => throw new NotImplementedException();

        public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text) => -1;

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength) => throw new NotImplementedException();

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            => throw new NotImplementedException();

        public override bool WillEncode(int unicodeScalar) => throw new NotImplementedException();
    }

    [Test]
    public void Test()
    {
        var encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

        Assert.That(JsonEncodedText.Encode("\"Test\""u8, encoder).Value, Is.EqualTo("\\\"Test\\\""));

        var encoded = JsonEncodedText.Encode("\"Test\""u8, DisableJavaScriptEncoder.Instance);
        Assert.That(encoded.Value, Is.EqualTo("\"Test\""));
    }
}