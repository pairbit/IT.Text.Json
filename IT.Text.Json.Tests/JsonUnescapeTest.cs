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
    }

    private static void Test(string str, string escaped)
    {
        Assert.That(JavaScriptEncoder.Default.Encode(str), Is.EqualTo(escaped));

        UnescapeTest(escaped, str);
    }

    private static void UnescapeTest(string escaped, string str)
    {
        Assert.That(Unescape(escaped), Is.EqualTo(str));
    }

    private static string Unescape(string escaped) 
        => Encoding.UTF8.GetString(Json.Unescape(Encoding.UTF8.GetBytes(escaped)));
}