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

public class StrictEnumJsonConverterTest
{
    [Test]
    public void BaseTest()
    {
        Assert.That(JsonSerializer.Serialize(EnumByte.First), Is.EqualTo("1"));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second), Is.EqualTo("2"));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("1"), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("2"), Is.EqualTo(EnumByte.Second));
    }

    [Test]
    public void StringEnum_Test()
    {
        var stringEnum = new JsonSerializerOptions();
        stringEnum.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        Assert.That(JsonSerializer.Serialize(EnumByte.First, stringEnum), Is.EqualTo("\"first\""));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second, stringEnum), Is.EqualTo("\"second\""));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"first\"", stringEnum), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"second\"", stringEnum), Is.EqualTo(EnumByte.Second));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"fiRsT\"", stringEnum), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"SeCoNd\"", stringEnum), Is.EqualTo(EnumByte.Second));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("1", stringEnum), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("2", stringEnum), Is.EqualTo(EnumByte.Second));
    }

    [Test]
    public void StringEnum_Flags_Test()
    {
        var stringEnum = new JsonSerializerOptions();
        stringEnum.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second, stringEnum),
            Is.EqualTo("\"first, second\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.First | EnumByteFlags.Second | EnumByteFlags.Four, stringEnum),
            Is.EqualTo("\"first, second, four\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("4", stringEnum), Is.EqualTo(EnumByteFlags.Four));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"first, second, four\"", stringEnum),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"fIrSt, seCoNd, foUr\"", stringEnum),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("7", stringEnum),
            Is.EqualTo((EnumByteFlags)7));
    }
}