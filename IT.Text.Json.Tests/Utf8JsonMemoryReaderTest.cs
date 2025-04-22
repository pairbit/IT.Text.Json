using System.Text.Json;

namespace IT.Text.Json.Tests;

internal class Utf8JsonMemoryReaderTest
{
    [Test]
    public void ReadArrayTest()
    {
        var utf8Json = "[true,false,67,null,\"MyStr\",\"Escaped\\\"Str\"]"u8.ToArray();
        var reader = new Utf8JsonMemoryReader(utf8Json);

        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.None));
        Assert.That(reader.ValueMemory.IsEmpty, Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.StartArray));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("["u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.True));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("true"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.False));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("false"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.Number));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("67"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.Null));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("null"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.String));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("MyStr"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.String));
        Assert.That(reader.ValueIsEscaped, Is.True);
        Assert.That(reader.ValueSpan.SequenceEqual("Escaped\\\"Str"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.EndArray));
        Assert.That(reader.ValueSpan.SequenceEqual("]"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.False);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.EndArray));
    }

    [Test]
    public void ReadObjectTest()
    {
        var utf8Json = "{\"prop\":13}"u8.ToArray();
        var reader = new Utf8JsonMemoryReader(utf8Json);

        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.None));
        Assert.That(reader.ValueMemory.IsEmpty, Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.StartObject));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("{"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.PropertyName));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("prop"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.Number));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("13"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.EndObject));
        Assert.That(reader.ValueSpan.SequenceEqual("}"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.False);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.EndObject));
    }

    [Test]
    public void ReadObjectWithCommentTest()
    {
        var utf8Json = @"{
//comment line
""prop"":13
/*
Multi comment lines
Comment line 1
Comment line 2
*/
}"u8.ToArray();
        var reader = new Utf8JsonMemoryReader(utf8Json, new JsonReaderOptions
        {
            CommentHandling = JsonCommentHandling.Allow
        });

        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.None));
        Assert.That(reader.ValueMemory.IsEmpty, Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.StartObject));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("{"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.Comment));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("comment line"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.PropertyName));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("prop"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.Number));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual("13"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.Comment));
        Assert.That(reader.ValueIsEscaped, Is.False);
        Assert.That(reader.ValueSpan.SequenceEqual(@"
Multi comment lines
Comment line 1
Comment line 2
"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.EndObject));
        Assert.That(reader.ValueSpan.SequenceEqual("}"u8), Is.True);
        Assert.That(reader.ValueMemory.Span.SequenceEqual(reader.ValueSpan), Is.True);

        Assert.That(reader.Read(), Is.False);
        Assert.That(reader.TokenType, Is.EqualTo(JsonTokenType.EndObject));
    }
}