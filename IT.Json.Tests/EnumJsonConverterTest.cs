using IT.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Tests;

enum EnumByte : byte
{
    None = 0,
    First = 1,
    Second = 2
}

[Flags]
enum EnumByteFlags : byte
{
    None = 0,
    First = 1,
    Second = 2,
    Four = 4
}

public class EnumJsonConverterTest
{
    [Test]
    public void Default_Test()
    {
        Assert.That(JsonSerializer.Serialize(EnumByte.First), Is.EqualTo("1"));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second), Is.EqualTo("2"));
        Assert.That(JsonSerializer.Serialize((EnumByte)200), Is.EqualTo("200"));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("1"), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("2"), Is.EqualTo(EnumByte.Second));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("200"), Is.EqualTo((EnumByte)200));
    }

    [Test]
    public void StringEnum_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        Assert.That(JsonSerializer.Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));
        Assert.That(JsonSerializer.Serialize((EnumByte)200, jso), Is.EqualTo("200"));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"fiRsT\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"SeCoNd\"", jso), Is.EqualTo(EnumByte.Second));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("1", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("2", jso), Is.EqualTo(EnumByte.Second));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("200", jso), Is.EqualTo((EnumByte)200));
    }

    [Test]
    public void StringEnum_NoInteger_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        Assert.That(JsonSerializer.Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"fiRsT\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"SeCoNd\"", jso), Is.EqualTo(EnumByte.Second));
    }

    [Test]
    public void StrictEnum_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new StrictEnumJsonConverter<EnumByte>(JsonNamingPolicy.CamelCase));

        Assert.That(JsonSerializer.Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));
    }

    [Test]
    public void StringEnum_Flags_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second, jso),
            Is.EqualTo("\"first, second\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second | EnumByteFlags.Four, jso),
            Is.EqualTo("\"first, second, four\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("4", jso), Is.EqualTo(EnumByteFlags.Four));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"first, second, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"fIrSt, seCoNd, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("7", jso),
            Is.EqualTo((EnumByteFlags)7));
    }

    [Test]
    public void StringEnum_Flags_NoInteger_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second, jso),
            Is.EqualTo("\"first, second\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second | EnumByteFlags.Four, jso),
            Is.EqualTo("\"first, second, four\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"first, second, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"fIrSt, seCoNd, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));
    }

    [Test]
    public void StrictEnum_Flags_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new StrictEnumJsonConverter<EnumByteFlags>(JsonNamingPolicy.CamelCase));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First, jso), Is.EqualTo("\"first\""));
        Assert.That(JsonSerializer.Serialize(EnumByteFlags.Second, jso), Is.EqualTo("\"second\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"first\"", jso), Is.EqualTo(EnumByteFlags.First));
        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"second\"", jso), Is.EqualTo(EnumByteFlags.Second));

        //Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second, jso),
        //    Is.EqualTo("\"first, second\""));

        //Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second | EnumByteFlags.Four, jso),
        //    Is.EqualTo("\"first, second, four\""));

        //Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"first, second, four\"", jso),
        //    Is.EqualTo((EnumByteFlags)7));
    }
}