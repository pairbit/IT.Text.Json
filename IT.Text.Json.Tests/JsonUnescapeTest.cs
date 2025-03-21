using System.Text;
using System.Text.Encodings.Web;

namespace IT.Text.Json.Tests;

internal class JsonUnescapeTest
{
    [Test]
    public void Test()
    {
        Test("mystr");
        Test("\"mystr\"");
        Test("\n\r\t");
    }

    [Test]
    public void UnescapeTest()
    {
        UnescapeTest("\\\"mystr\\\"", "\"mystr\"");
        UnescapeTest("\\n\\r\\t", "\n\r\t");
    }

    private static void Test(string str)
    {
        var encoded = JavaScriptEncoder.Default.Encode(str);
        var unescaped = Unescape(encoded);
        Assert.That(unescaped, Is.EqualTo(str));
    }

    private static void UnescapeTest(string escaped, string unescaped)
    {
        Assert.That(Unescape(escaped), Is.EqualTo(unescaped));
    }

    private static string Unescape(string escaped) 
        => Encoding.UTF8.GetString(Json.Unescape(Encoding.UTF8.GetBytes(escaped)));
}