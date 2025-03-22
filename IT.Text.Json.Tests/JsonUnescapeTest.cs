using System.Text;
using System.Text.Encodings.Web;

namespace IT.Text.Json.Tests;

internal class JsonUnescapeTest
{
    [Test]
    public void Test()
    {
        Test("mystr", "mystr");
        Test("/", "/");

        Test("\"mystr\"", "\\u0022mystr\\u0022");
        Test("\n\r\t\b\f", "\\n\\r\\t\\b\\f");
        Test("\\", "\\\\");

        Test("моя строка", "\\u043C\\u043E\\u044F \\u0441\\u0442\\u0440\\u043E\\u043A\\u0430");
    }

    [Test]
    public void UnescapeTest()
    {
        UnescapeTest("\\/", "/");
        UnescapeTest("\\\"mystr\\\"", "\"mystr\"");
        UnescapeTest("\\u0022mystr\\u0022", "\"mystr\"");

        Assert.Throws<IndexOutOfRangeException>(() => Json.Unescape("\\"u8));
    }

    private static void Test(string str, string escaped)
    {
        Assert.That(JavaScriptEncoder.Default.Encode(str), Is.EqualTo(escaped));

        UnescapeTest(escaped, str);
    }

    private static void UnescapeTest(string escaped, string str)
    {
        var escapedUtf8 = Encoding.UTF8.GetBytes(escaped);
        var unescapedUtf8 = Json.Unescape(escapedUtf8);

        var unescaped = Encoding.UTF8.GetString(unescapedUtf8);
        Assert.That(unescaped, Is.EqualTo(str));

        Json.UnescapeInPlace(escapedUtf8, out var written);

        unescaped = Encoding.UTF8.GetString(escapedUtf8.AsSpan(0, written));
        Assert.That(unescaped, Is.EqualTo(str));
    }
}