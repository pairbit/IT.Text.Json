using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Benchmarks;

[Flags]
public enum EnumByteFlags : byte
{
    None = 0,
    One = 1,
    Two = 2,
}

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class FlagsEnumJsonConverterBenchmark
{
    private static JsonSerializerOptions _jsoStringCamelCase = null!;
    private static JsonSerializerOptions _jsoStringCamelCaseInteger = null!;
    private static JsonSerializerOptions _jsoStringCamelCaseStrict = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jsoStringCamelCase = new JsonSerializerOptions();
        _jsoStringCamelCase.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jsoStringCamelCaseInteger = new JsonSerializerOptions();
        _jsoStringCamelCaseInteger.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));

        _jsoStringCamelCaseStrict = new JsonSerializerOptions();
        _jsoStringCamelCaseStrict.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));
    }

    [Benchmark]
    public string Serialize_Number() => JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two);

    [Benchmark]
    public string Serialize_String() => JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two, _jsoStringCamelCase);

    [Benchmark]
    public string Serialize_Strict() => JsonSerializer.Serialize(EnumByteFlags.One | EnumByteFlags.Two, _jsoStringCamelCaseStrict);
    
    [Benchmark]
    public EnumByteFlags Deserialize_Number() => JsonSerializer.Deserialize<EnumByteFlags>("3");

    [Benchmark]
    public EnumByteFlags Deserialize_String() => JsonSerializer.Deserialize<EnumByteFlags>("\"one, two\"", _jsoStringCamelCase);

    [Benchmark]
    public EnumByteFlags Deserialize_Strict() => JsonSerializer.Deserialize<EnumByteFlags>("\"one, two\"", _jsoStringCamelCaseStrict);

    public void Test()
    {
        GlobalSetup();

        if (Serialize_Number() != "3") throw new InvalidOperationException("Serialize_Number");
        if (Serialize_String() != "\"one, two\"") throw new InvalidOperationException("Serialize_String");
        if (Serialize_Strict() != "\"one, two\"") throw new InvalidOperationException("Serialize_Strict");

        var three = EnumByteFlags.One | EnumByteFlags.Two;
        if (Deserialize_Number() != three) throw new InvalidOperationException("Deserialize_Number");
        if (Deserialize_String() != three) throw new InvalidOperationException("Deserialize_String");
        if (Deserialize_Strict() != three) throw new InvalidOperationException("Deserialize_Strict");
    }
}