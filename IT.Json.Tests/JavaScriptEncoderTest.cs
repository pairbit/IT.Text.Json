using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace IT.Json.Tests;

public class JavaScriptEncoderTest
{
    [Test]
    public void Test()
    {
        var data = "\"Test\""u8.ToArray();

        Assert.That(JsonEncodedText.Encode(data, JavaScriptEncoder.UnsafeRelaxedJsonEscaping).Value, 
            Is.EqualTo("\\\"Test\\\""));

        var encoded = JsonEncodedText.Encode(data, DisabledJavaScriptEncoder.Instance);
        Assert.That(encoded.Value, Is.EqualTo("\"Test\""));
        Assert.That(encoded.EncodedUtf8Bytes.SequenceEqual(data), Is.True);

        var encoded2 = GetEncodedText(data);
        Assert.That(encoded2.Value, Is.Empty);
        Assert.That(encoded2.EncodedUtf8Bytes.SequenceEqual(data), Is.True);
    }

    public static JsonEncodedText GetEncodedText(byte[] data)
    {
        if (Unsafe.SizeOf<JsonEncodedText>() != 16) 
            throw new NotSupportedException($"sizeOf {typeof(JsonEncodedText).FullName} is not 16");

        var notEncoded = new JsonNotEncodedText(data);
        return Unsafe.As<JsonNotEncodedText, JsonEncodedText>(ref notEncoded);
    }

    readonly struct JsonNotEncodedText
    {
        readonly byte[] _utf8Value;
        readonly string _value;

        public JsonNotEncodedText(byte[] utf8Value)
        {
            _utf8Value = utf8Value;
            _value = string.Empty;
        }
    }

    class DisabledJavaScriptEncoder : JavaScriptEncoder
    {
        public static readonly DisabledJavaScriptEncoder Instance = new();

        public override int MaxOutputCharactersPerInputCharacter => throw new NotImplementedException();

        public override int FindFirstCharacterToEncodeUtf8(ReadOnlySpan<byte> utf8Text) => -1;

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength) => throw new NotImplementedException();

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
            => throw new NotImplementedException();

        public override bool WillEncode(int unicodeScalar) => throw new NotImplementedException();
    }
}