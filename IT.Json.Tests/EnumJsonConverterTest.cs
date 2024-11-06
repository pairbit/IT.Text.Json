﻿using IT.Json.Converters;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Tests;

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
    public void Default_Test()
    {
        Assert.That(JsonSerializer.Serialize(EnumByte.First), Is.EqualTo("1"));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second), Is.EqualTo("2"));
        Assert.That(JsonSerializer.Serialize((EnumByte)200), Is.EqualTo("200"));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("1"), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>(" 2  "), Is.EqualTo(EnumByte.Second));
        Assert.That(JsonSerializer.Deserialize<EnumByte>(" 200"), Is.EqualTo((EnumByte)200));
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

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"  fiRsT  \"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"SeCoNd  \"", jso), Is.EqualTo(EnumByte.Second));

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

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"   fiRsT\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\" SeCoNd   \"", jso), Is.EqualTo(EnumByte.Second));
    }

    [Test]
    public void StrictEnum_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverter<EnumByte>(JsonNamingPolicy.CamelCase));
        jso.Converters.Add(new EnumJsonConverter<EnumOne>(JsonNamingPolicy.CamelCase));

        Assert.That(JsonSerializer.Serialize(EnumByte.First, jso), Is.EqualTo("\"first\""));
        Assert.That(JsonSerializer.Serialize(EnumByte.Second, jso), Is.EqualTo("\"second\""));
        Assert.That(JsonSerializer.Serialize(EnumOne.None, jso), Is.EqualTo("\"none\""));

        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"first\"", jso), Is.EqualTo(EnumByte.First));
        Assert.That(JsonSerializer.Deserialize<EnumByte>("\"second\"", jso), Is.EqualTo(EnumByte.Second));
        Assert.That(JsonSerializer.Deserialize<EnumOne>("\"none\"", jso), Is.EqualTo(EnumOne.None));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumByte)109, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("109").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Deserialize<EnumByte>("\"four\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("four").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Deserialize<EnumByte>("\"fIrSt\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("fIrSt").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Deserialize<EnumByte>("\" second \"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>(" second ").Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new EnumJsonConverter<EnumEmpty>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumEmpty<EnumEmpty>().Message));
    }

    [Test]
    public void StringEnum_Flags_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.None, jso),
            Is.EqualTo("\"none\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
            Is.EqualTo("\"two, five\""));

        //Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two | (EnumByteFlags)8, jso),
        //    Is.EqualTo("\"first, second, eight\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("4", jso), Is.EqualTo(EnumByteFlags.Four));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"  two ,    five    \"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"  one   , two ,    four    \"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"three, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"tHrEe, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"  oNe, tWO, ThRee, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("  7  ", jso),
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
            Is.EqualTo("\"two, five\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"  one   ,    two, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"OnE, tWO, foUr\"", jso),
            Is.EqualTo((EnumByteFlags)7));
    }

    [Test]
    public void StrictEnum_Flags_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new FlagsEnumJsonConverter<EnumByteFlags, byte>(JsonNamingPolicy.CamelCase));
        jso.Converters.Add(new FlagsEnumJsonConverter<EnumIntFlags, int>(JsonNamingPolicy.CamelCase));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(JsonSerializer.Serialize(EnumByteFlags.Two, jso), Is.EqualTo("\"two\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"one\"", jso), Is.EqualTo(EnumByteFlags.One));
        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"two\"", jso), Is.EqualTo(EnumByteFlags.Two));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.Four | EnumByteFlags.One, jso),
            Is.EqualTo("\"five\""));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
            Is.EqualTo("\"two, five\""));

        //Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"one, two, four\"", jso),
        //    Is.EqualTo((EnumByteFlags)7));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumByteFlags)8, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("8").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumByteFlags)109, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("109").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumIntFlags)2, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumIntFlags>("2").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumIntFlags)6, jso)).Message,
            Is.EqualTo(JsonNotMappedBit<EnumIntFlags>("6", "2").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumIntFlags)10, jso)).Message,
            Is.EqualTo(JsonNotMappedBit<EnumIntFlags>("10", "2").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Serialize((EnumIntFlags)16, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumIntFlags>("16").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Deserialize<EnumByteFlags>("\"six\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("six").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Deserialize<EnumByteFlags>("\"OnE\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("OnE").Message));

        Assert.That(Assert.Catch<JsonException>(() => JsonSerializer.Deserialize<EnumByteFlags>("\" two \"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>(" two ").Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumOne, uint>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumNotBase<EnumOne>().Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumEmpty, int>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumEmpty<EnumEmpty>().Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumOne, int>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumMoreOne<EnumOne>().Message));
    }

    [Test]
    public void StrictEnum_Flags_Factory_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase, 0, "|"u8.ToArray()));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(JsonSerializer.Serialize(EnumInt.x1, jso), Is.EqualTo("\"11111111111111111111111111111111111111111\""));

        Assert.That(JsonSerializer.Deserialize<EnumByteFlags>("\"one\"", jso), Is.EqualTo(EnumByteFlags.One));
        Assert.That(JsonSerializer.Deserialize<EnumInt>("\"11111111111111111111111111111111111111111\"", jso), Is.EqualTo(EnumInt.x1));

        Assert.That(JsonSerializer.Serialize(EnumByteFlags.Two | EnumByteFlags.Five, jso),
            Is.EqualTo("\"two|five\""));

        Assert.That(JsonSerializer.Serialize(EnumInt.x2 | EnumInt.x8, jso),
            Is.EqualTo("\"22222222222222222222222222222222222222222|8888888888888888888888888888888888888888888\""));
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