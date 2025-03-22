using IT.Text.Json.Converters;
using IT.Text.Json.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Tests;

enum EnumByte : byte
{
    None = 0,
    First = 1,
    Second = 2,
    x3 = 3,
    x4 = 4,
    x5 = 5,
    x6 = 6,
    x7 = 7,
    x8 = 8,
    x9 = 9,
    x10 = 10
}

[Flags]
enum EnumByteFlags : byte
{
    None = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    //Eight = 8
}

enum EnumIntFlags
{
    None = 0,
    One = 1,
    //Two = 2,
    Three = 3,
    Four = 4,
    Eight = 8
}

[Flags]
enum EnumInt
{
    [JsonPropertyName("00000000000000000000000000000000000000000")] x0,
    [JsonPropertyName("11111111111111111111111111111111111111111")] x1,
    [JsonPropertyName("22222222222222222222222222222222222222222")] x2,
    [JsonPropertyName("33333333333333333333333333333333333333333")] x3,
    [JsonPropertyName("44444444444444444444444444444444444444444")] x4,
    [JsonPropertyName("555555555555555555555555555555555555555555")] x5,
    [JsonPropertyName("666666666666666666666666666666666666666666")] x6,
    [JsonPropertyName("777777777777777777777777777777777777777777")] x7,
    [JsonPropertyName("8888888888888888888888888888888888888888888")] x8,
    [JsonPropertyName("99999999999999999999999999999999999999999999")] x9
}

enum EnumEmpty { }

enum EnumOne
{
    None = 0
}

[Flags]
enum EnumEscaped
{
    [JsonPropertyName("\"Escaped\"")]
    Escaped = 4,

    [JsonPropertyName("\"Escaped__2\"")]
    Escaped2 = 8,
}

[Flags]
enum EnumMemberName
{
#if NET8_0_OR_GREATER
    [JsonStringEnumMemberName("enumMemberName1")]
#else
    [JsonPropertyName("enumMemberName1")]
#endif
    Value1 = 4,

#if NET8_0_OR_GREATER
    [JsonStringEnumMemberName("enumMemberName2")]
#else
    [JsonPropertyName("enumMemberName2")]
#endif
    Value2 = 8,
}

public class EnumJsonConverterTest
{
    [Test]
    public void Base_Test()
    {
        var eb = EnumByte.Second;
        Assert.That(Unsafe.As<EnumByte, byte>(ref eb), Is.EqualTo(2));

        Assert.That(EnumByteFlags.None |
            EnumByteFlags.One | EnumByteFlags.Two |
            EnumByteFlags.Three | EnumByteFlags.Four |
            EnumByteFlags.Five, Is.EqualTo((EnumByteFlags)7));
    }

    [Test]
    public void IndexOfPart_Test()
    {
        var sep = ", |"u8;

        var s = "1, |2"u8;
        var index = s.IndexOfPart(sep, out var length);
        Assert.That(index, Is.EqualTo(1));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = ", |"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(0));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "sdfsdfsdfsdfsdfsdfdsfsdf , |"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(25));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "sdfsdfsdfsdfsdfsdfdsfsdf , sdfsdfsdfsdfsdfsdfds, , |fsdf"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(49));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "1, 2"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(-1));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(0));

        s = " |1, 2"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(-1));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(0));

        s = "1, 2, |3"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(4));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "1,, |2"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(2));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "1, , |2"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(3));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "1, 2,"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(4));
        Assert.That(length, Is.EqualTo(1));
        Assert.That(s.IndexOf(sep), Is.EqualTo(-1));

        s = "1, 2, "u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(4));
        Assert.That(length, Is.EqualTo(2));
        Assert.That(s.IndexOf(sep), Is.EqualTo(-1));

        sep = ","u8;

        s = "12,2"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(2));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        s = "123"u8;
        index = s.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(-1));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(0));
    }

    [Test]
    public void Segments()
    {
        var sep = ", |"u8;
        var sepPart = sep;

        var s1 = "1,"u8;
        var index = s1.IndexOfPart(sep, out var length);
        Assert.That(index, Is.EqualTo(1));
        Assert.That(length, Is.EqualTo(1));

        sepPart = sep.Slice(length);

        var s2 = " |2"u8;
        Assert.That(s2.StartsWith(sepPart), Is.True);

        s1 = "1, "u8;
        index = s1.IndexOfPart(sep, out length);
        Assert.That(index, Is.EqualTo(1));
        Assert.That(length, Is.EqualTo(2));

        s2 = "|2"u8;

        sepPart = sep.Slice(length);
        Assert.That(s2.StartsWith(sepPart), Is.True);
    }

    [Test]
    public void Default_Test()
    {
        Assert.That(Serialize(EnumByte.First), Is.EqualTo("1"));
        Assert.That(Serialize(EnumByte.Second), Is.EqualTo("2"));
        Assert.That(Serialize((EnumByte)200), Is.EqualTo("200"));

        Assert.That(Deserialize<EnumByte>("1"), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>(" 2  "), Is.EqualTo(EnumByte.Second));
        Assert.That(Deserialize<EnumByte>(" 200"), Is.EqualTo((EnumByte)200));
    }

    [Test]
    public void StringEnum_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        Assert.That(Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));
        Assert.That(Serialize((EnumByte)200, jso), Is.EqualTo("200"));
        
        Assert.That(Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));

        Assert.That(Deserialize<EnumByte>("\"  fiRsT  \"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>("\"SeCoNd  \"", jso), Is.EqualTo(EnumByte.Second));

        Assert.That(Deserialize<EnumByte>("1", jso), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>("2", jso), Is.EqualTo(EnumByte.Second));
        Assert.That(Deserialize<EnumByte>("200", jso), Is.EqualTo((EnumByte)200));
    }

    [Test]
    public void StringEnum_NoInteger_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        Assert.That(Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));

        Assert.That(Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));

        Assert.That(Deserialize<EnumByte>("\"   fiRsT\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>("\" SeCoNd   \"", jso), Is.EqualTo(EnumByte.Second));

#if NET8_0_OR_GREATER
        Assert.That(Serialize(EnumMemberName.Value1, jso), Is.EqualTo("\"enumMemberName1\""));
        Assert.That(Serialize(EnumMemberName.Value2, jso), Is.EqualTo("\"enumMemberName2\""));

        Assert.That(Deserialize<EnumMemberName>("\"enumMemberName1\"", jso), Is.EqualTo(EnumMemberName.Value1));
        Assert.That(Deserialize<EnumMemberName>("\"enumMemberName2\"", jso), Is.EqualTo(EnumMemberName.Value2));
#else
        Assert.That(Serialize(EnumMemberName.Value1, jso), Is.EqualTo("\"value1\""));
        Assert.That(Serialize(EnumMemberName.Value2, jso), Is.EqualTo("\"value2\""));

        Assert.That(Deserialize<EnumMemberName>("\"value1\"", jso), Is.EqualTo(EnumMemberName.Value1));
        Assert.That(Deserialize<EnumMemberName>("\"value2\"", jso), Is.EqualTo(EnumMemberName.Value2));
#endif
    }

    [Test]
    public void StrictEnum_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverter<EnumByte>(JsonNamingPolicy.CamelCase));
        jso.Converters.Add(new EnumJsonConverter<EnumOne>(JsonNamingPolicy.CamelCase));

        Assert.That(Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));
        Assert.That(Serialize(EnumOne.None, jso), Is.EqualTo("\"none\""));

        Assert.That(Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));
        Assert.That(Deserialize<EnumOne>("\"none\"", jso), Is.EqualTo(EnumOne.None));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumByte)109, jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("109").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByte>("\"four\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("four").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByte>("\"fIrSt\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("fIrSt").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByte>("\" second \"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByte>(" second ").Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new EnumJsonConverter<EnumEmpty>(JsonNamingPolicy.CamelCase))!.GetBaseException().Message,
            Is.EqualTo(ArgEnumEmpty<EnumEmpty>().Message));
    }
    

    [Test]
    public void StringEnum_Flags_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        Assert.That(Serialize(EnumByteFlags.None, jso),
            Is.EqualTo("\"none\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
#if NET8_0_OR_GREATER
            Is.EqualTo("\"one, two, four\""));
#else
            Is.EqualTo("\"two, five\""));
#endif
        //Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two | (EnumByteFlags)8, jso),
        //    Is.EqualTo("\"first, second, eight\""));

        Assert.That(Deserialize<EnumByteFlags>("4", jso), Is.EqualTo(EnumByteFlags.Four));

        Assert.That(Deserialize<EnumByteFlags>("\"  two ,    five    \"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"  one   , two ,    four    \"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"three, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"tHrEe, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"  oNe, tWO, ThRee, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("  7  ", jso),
            Is.EqualTo((EnumByteFlags)7));
    }

    [Test]
    public void StringEnum_Flags_NoInteger_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
#if NET8_0_OR_GREATER
            Is.EqualTo("\"one, two, four\""));
#else
            Is.EqualTo("\"two, five\""));
#endif

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"  one   ,    two, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"OnE, tWO, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));
    }

#if NET7_0_OR_GREATER
    [Test]
    public void StrictEnum_Flags_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new FlagsEnumJsonConverter<EnumByteFlags, byte>(JsonNamingPolicy.CamelCase));
        jso.Converters.Add(new FlagsEnumJsonConverter<EnumIntFlags, int>(JsonNamingPolicy.CamelCase));

        Assert.That(Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(Serialize(EnumByteFlags.Two, jso), Is.EqualTo("\"two\""));

        Assert.That(Deserialize<EnumByteFlags>("\"one\"", jso), Is.EqualTo(EnumByteFlags.One));
        Assert.That(Deserialize<EnumByteFlags>("\"two\"", jso), Is.EqualTo(EnumByteFlags.Two));

        Assert.That(Deserialize<EnumByteFlags>("\"two, five\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"three, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"one, two, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"one, two, three, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumByteFlags)8, jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("8").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumByteFlags)109, jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("109").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)2, jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumIntFlags>("2").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)6, jso))!.Message,
            Is.EqualTo(JsonNotMappedBit<EnumIntFlags>("6", "2").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)10, jso))!.Message,
            Is.EqualTo(JsonNotMappedBit<EnumIntFlags>("10", "2").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)16, jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumIntFlags>("16").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"six\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("six").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"OnE\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("OnE").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\" two \"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>(" two ").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"onef, two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("onef").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one, two4\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two4").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one111, two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("one111").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one, two444\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two444").Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumOne, uint>(JsonNamingPolicy.CamelCase))!.GetBaseException().Message,
            Is.EqualTo(ArgEnumNotBase<EnumOne>().Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumEmpty, int>(JsonNamingPolicy.CamelCase))!.GetBaseException().Message,
            Is.EqualTo(ArgEnumEmpty<EnumEmpty>().Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumOne, int>(JsonNamingPolicy.CamelCase))!.GetBaseException().Message,
            Is.EqualTo(ArgEnumMoreOne<EnumOne>().Message));
    }

    [Test]
    public void StrictEnum_Flags_SerializeFromEnd_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new FlagsEnumJsonConverter<EnumByteFlags, byte>(JsonNamingPolicy.CamelCase,
            writeFromEnd: true));

        Assert.That(Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(Serialize(EnumByteFlags.Two, jso), Is.EqualTo("\"two\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(Serialize(EnumByteFlags.Four | EnumByteFlags.One, jso),
            Is.EqualTo("\"five\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
            Is.EqualTo("\"two, five\""));
    }

    [Test]
    public void StrictEnum_Flags_SerializeFromStart_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new FlagsEnumJsonConverter<EnumByteFlags, byte>(JsonNamingPolicy.CamelCase,
            writeFromEnd: false));

        Assert.That(Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(Serialize(EnumByteFlags.Two, jso), Is.EqualTo("\"two\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(Serialize(EnumByteFlags.Four | EnumByteFlags.One, jso),
            Is.EqualTo("\"five\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
            Is.EqualTo("\"one, two, four\""));
    }

    [Test]
    public void StrictEnum_Factory_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase, jso.Encoder, 234, "|"u8.ToArray()));

        Assert.That(Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(Serialize(EnumInt.x1, jso), Is.EqualTo("\"11111111111111111111111111111111111111111\""));

        Assert.That(Deserialize<EnumByteFlags>("\"one\"", jso), Is.EqualTo(EnumByteFlags.One));
        Assert.That(Deserialize<EnumInt>("\"11111111111111111111111111111111111111111\"", jso), Is.EqualTo(EnumInt.x1));

        Assert.That(Serialize(EnumByteFlags.Two | EnumByteFlags.Five, jso),
            Is.EqualTo("\"two|five\""));

        Assert.That(Deserialize<EnumByteFlags>("\"two|five\"", jso),
            Is.EqualTo(EnumByteFlags.Two | EnumByteFlags.Five));

        Assert.That(Serialize(EnumInt.x2 | EnumInt.x8, jso),
            Is.EqualTo("\"22222222222222222222222222222222222222222|8888888888888888888888888888888888888888888\""));

        Assert.That(Serialize(EnumEscaped.Escaped, jso),
            Is.EqualTo("\"\\u0022Escaped\\u0022\""));

        Assert.That(Serialize(EnumEscaped.Escaped | EnumEscaped.Escaped2, jso),
            Is.EqualTo("\"\\u0022Escaped\\u0022|\\u0022Escaped__2\\u0022\""));

        Assert.That(Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022\"", jso),
            Is.EqualTo(EnumEscaped.Escaped));

        Assert.That(Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022|\\u0022Escaped__2\\u0022\"", jso),
            Is.EqualTo(EnumEscaped.Escaped | EnumEscaped.Escaped2));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one34|two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("one34").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one|two444\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two444").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one|4445|two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("4445").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one|two|sdf\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("sdf").Message));
    }

    [Test]
    public void StrictEnum_Factory_SepEncoded_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase, jso.Encoder, sep: "\"|"u8.ToArray()));

        Assert.That(Serialize(EnumByteFlags.Two | EnumByteFlags.Five, jso),
            Is.EqualTo("\"two\\u0022|five\""));

        Assert.That(Deserialize<EnumByteFlags>("\"two\\u0022|five\"", jso),
            Is.EqualTo(EnumByteFlags.Two | EnumByteFlags.Five));

        Assert.That(Serialize(EnumInt.x2 | EnumInt.x8, jso),
            Is.EqualTo("\"22222222222222222222222222222222222222222\\u0022|8888888888888888888888888888888888888888888\""));

        Assert.That(Serialize(EnumEscaped.Escaped, jso),
            Is.EqualTo("\"\\u0022Escaped\\u0022\""));

        Assert.That(Serialize(EnumEscaped.Escaped | EnumEscaped.Escaped2, jso),
            Is.EqualTo("\"\\u0022Escaped\\u0022\\u0022|\\u0022Escaped__2\\u0022\""));

        Assert.That(Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022\"", jso),
            Is.EqualTo(EnumEscaped.Escaped));

        Assert.That(Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022\\u0022|\\u0022Escaped__2\\u0022\"", jso),
            Is.EqualTo(EnumEscaped.Escaped | EnumEscaped.Escaped2));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one34\\u0022|two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("one34").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one\\u0022|two444\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two444").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one\\u0022|4445\\u0022|two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("4445").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one\\u0022|two\\u0022|sdf\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("sdf").Message));
    }

    [Test]
    public void StrictEnum_Factory_Sep4_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase, jso.Encoder, 234, "||||"u8.ToArray()));

        Assert.That(Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(Serialize(EnumInt.x1, jso), Is.EqualTo("\"11111111111111111111111111111111111111111\""));

        Assert.That(Deserialize<EnumByteFlags>("\"one\"", jso), Is.EqualTo(EnumByteFlags.One));
        Assert.That(Deserialize<EnumInt>("\"11111111111111111111111111111111111111111\"", jso), Is.EqualTo(EnumInt.x1));

        Assert.That(Serialize(EnumByteFlags.Two | EnumByteFlags.Five, jso),
            Is.EqualTo("\"two||||five\""));

        Assert.That(Deserialize<EnumByteFlags>("\"two||||five\"", jso),
            Is.EqualTo(EnumByteFlags.Two | EnumByteFlags.Five));

        Assert.That(Serialize(EnumInt.x2 | EnumInt.x8, jso),
            Is.EqualTo("\"22222222222222222222222222222222222222222||||8888888888888888888888888888888888888888888\""));

        Assert.That(Serialize(EnumEscaped.Escaped, jso),
            Is.EqualTo("\"\\u0022Escaped\\u0022\""));

        Assert.That(Serialize(EnumEscaped.Escaped | EnumEscaped.Escaped2, jso),
            Is.EqualTo("\"\\u0022Escaped\\u0022||||\\u0022Escaped__2\\u0022\""));

        Assert.That(Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022\"", jso),
            Is.EqualTo(EnumEscaped.Escaped));

        Assert.That(Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022||||\\u0022Escaped__2\\u0022\"", jso),
            Is.EqualTo(EnumEscaped.Escaped | EnumEscaped.Escaped2));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one34||||two\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("one34").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one||||two444\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two444").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one||||two||||sdf\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("sdf").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one|||two||||sdf\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("one|||two").Message));
    }

    [Test]
    public void StrictEnum_Factory_Sep2_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase, jso.Encoder, 111, "o|"u8.ToArray()));

        Assert.That(Serialize(EnumByteFlags.Two | EnumByteFlags.Five, jso),
            Is.EqualTo("\"twoo|five\""));

        Assert.That(Deserialize<EnumByteFlags>("\"twoo|five\"", jso),
            Is.EqualTo(EnumByteFlags.Two | EnumByteFlags.Five));

        Assert.That(Deserialize<EnumByteFlags>("\"fiveo|two\"", jso),
            Is.EqualTo(EnumByteFlags.Two | EnumByteFlags.Five));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"two|five\"", jso))!.Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("tw").Message));
    }
#endif

    private static string Serialize<T>(T value, JsonSerializerOptions? options = null)
    {
        var s1 = JsonSerializer.Serialize(value, options);
        var s2 = JsonSerializer.Serialize(value, options);
        if (!Equals(s1, s2)) throw new InvalidOperationException();

        return s2;
    }

    private static T? Deserialize<T>(
#if NET7_0_OR_GREATER
        [StringSyntax(StringSyntaxAttribute.Json)]
#endif
    string json, JsonSerializerOptions? options = null)
    {
        return JsonDeserializerTester.Deserialize<T>(json, options);
    }

    private static ArgumentException ArgEnumNotBase<TEnum>() =>
        new($"UnderlyingType enum '{typeof(TEnum).FullName}' is '{typeof(TEnum).GetEnumUnderlyingType().FullName}'", "TNumber");

    private static ArgumentException ArgEnumMoreOne<TEnum>() =>
        new($"Enum '{typeof(TEnum).FullName}' must contain more than one value", nameof(TEnum));

    private static ArgumentException ArgEnumEmpty<TEnum>() =>
        new($"Enum '{typeof(TEnum).FullName}' cannot be empty", nameof(TEnum));

    private static JsonException JsonNotMapped<TEnum>(string? value) =>
        new($"The JSON enum '{value}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.");

    private static JsonException JsonNotMappedBit<TEnum>(string? value, string bit) =>
        new($"The bit {bit} JSON enum '{value}' could not be mapped to any .NET member contained in type '{typeof(TEnum).FullName}'.");
}