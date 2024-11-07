using IT.Json.Converters;
using System.Diagnostics.CodeAnalysis;
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

enum EnumEscaped
{
    [JsonPropertyName("\"Escaped\"")]
    Escaped = 0
}

public static class Ext
{
    internal static int IndexOfPart<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> value, out int length)
        where T : IEquatable<T>?
    {
        if (span.Length == 0) throw new ArgumentException("span is empty", nameof(span));
        
        var maxLength = value.Length;
        if (maxLength == 0) throw new ArgumentException("value is empty", nameof(value));
        if (maxLength == 1) throw new ArgumentException("value too small", nameof(value));

        var index = -1;
        var len = 0;
        var v = value[0];
        for (int i = 0; i < span.Length; i++)
        {
            var s = span[i];
            if (EqualityComparer<T>.Default.Equals(v, s))
            {
                if (index == -1) index = i;
                if (++len == maxLength)
                {
                    length = len;
                    return index;
                }
                v = value[len];
            }
            else if (len > 0)
            {
                v = value[0];
                if (EqualityComparer<T>.Default.Equals(v, s))
                {
                    index = i;
                    len = 1;
                    v = value[1];
                }
                else
                {
                    index = -1;
                    len = 0;
                }
            }
        }
        length = len;
        return index;
    }
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
    }

    [Test]
    public void Segments()
    {
        var sep = ", |"u8;
        
        var s = "1, |2"u8;
        var index = s.IndexOfPart(sep, out var length);
        Assert.That(index, Is.EqualTo(1));
        Assert.That(index, Is.EqualTo(s.IndexOf(sep)));
        Assert.That(length, Is.EqualTo(sep.Length));

        var sepPart = sep;

        var s1 = "1,"u8;
        var s2 = " 2"u8;

        s1 = "1, "u8;
        s2 = "|2"u8;


        Assert.That(s1.EndsWith(sep));
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

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumByte)109, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("109").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByte>("\"four\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("four").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByte>("\"fIrSt\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>("fIrSt").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByte>("\" second \"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByte>(" second ").Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new EnumJsonConverter<EnumEmpty>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
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
            Is.EqualTo("\"two, five\""));

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

        Assert.That(Serialize(EnumByteFlags.One, jso), Is.EqualTo("\"one\""));
        Assert.That(Serialize(EnumByteFlags.Two, jso), Is.EqualTo("\"two\""));

        Assert.That(Deserialize<EnumByteFlags>("\"one\"", jso), Is.EqualTo(EnumByteFlags.One));
        Assert.That(Deserialize<EnumByteFlags>("\"two\"", jso), Is.EqualTo(EnumByteFlags.Two));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two, jso),
            Is.EqualTo("\"three\""));

        Assert.That(Serialize(EnumByteFlags.Four | EnumByteFlags.One, jso),
            Is.EqualTo("\"five\""));

        Assert.That(Serialize(EnumByteFlags.One | EnumByteFlags.Two | EnumByteFlags.Four, jso),
            Is.EqualTo("\"two, five\""));

        Assert.That(Deserialize<EnumByteFlags>("\"two, five\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"three, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"one, two, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Deserialize<EnumByteFlags>("\"one, two, three, four\"", jso),
            Is.EqualTo((EnumByteFlags)7));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumByteFlags)8, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("8").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumByteFlags)109, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("109").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)2, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumIntFlags>("2").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)6, jso)).Message,
            Is.EqualTo(JsonNotMappedBit<EnumIntFlags>("6", "2").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)10, jso)).Message,
            Is.EqualTo(JsonNotMappedBit<EnumIntFlags>("10", "2").Message));

        Assert.That(Assert.Catch<JsonException>(() => Serialize((EnumIntFlags)16, jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumIntFlags>("16").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"six\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("six").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"OnE\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("OnE").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\" two \"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>(" two ").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"onef, two\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("onef").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one, two4\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two4").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one111, two\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("one111").Message));

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumByteFlags>("\"one, two444\"", jso)).Message,
            Is.EqualTo(JsonNotMapped<EnumByteFlags>("two444").Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumOne, uint>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumNotBase<EnumOne>().Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumEmpty, int>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumEmpty<EnumEmpty>().Message));

        Assert.That(Assert.Catch<TypeInitializationException>(() => new FlagsEnumJsonConverter<EnumOne, int>(JsonNamingPolicy.CamelCase)).GetBaseException().Message,
            Is.EqualTo(ArgEnumMoreOne<EnumOne>().Message));
    }

    [Test]
    public void StrictEnum_Factory_Test()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase, 234, "|"u8.ToArray()));

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

        Assert.That(Assert.Catch<JsonException>(() => Deserialize<EnumEscaped>("\"\\u0022Escaped\\u0022\"", jso)).Message,
            Is.EqualTo("Escaped value is not supported"));
    }

    private static string Serialize<T>(T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options);
    }

    private static T? Deserialize<T>([StringSyntax(StringSyntaxAttribute.Json)] string json, JsonSerializerOptions? options = null)
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