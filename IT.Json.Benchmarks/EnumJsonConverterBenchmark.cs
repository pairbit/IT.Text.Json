using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Benchmarks;

public enum EnumByte : byte
{
    None = 0,
    One = 1,
    Two = 2
}

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class EnumJsonConverterBenchmark
{
    private static JsonSerializerOptions _jsoStringCamelCase = null!;
    private static JsonSerializerOptions _jsoStringCamelCaseStrict = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jsoStringCamelCase = new JsonSerializerOptions();
        _jsoStringCamelCase.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jsoStringCamelCaseStrict = new JsonSerializerOptions();
        _jsoStringCamelCaseStrict.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));
    }

    [Benchmark]
    public string Serialize_Number() => JsonSerializer.Serialize(EnumByte.Two);

    [Benchmark]
    public string Serialize_String() => JsonSerializer.Serialize(EnumByte.Two, _jsoStringCamelCase);

    [Benchmark]
    public string Serialize_Strict() => JsonSerializer.Serialize(EnumByte.Two, _jsoStringCamelCaseStrict);
    
    [Benchmark]
    public EnumByte Deserialize_Number() => JsonSerializer.Deserialize<EnumByte>("2");

    [Benchmark]
    public EnumByte Deserialize_String() => JsonSerializer.Deserialize<EnumByte>("\"two\"", _jsoStringCamelCase);

    [Benchmark]
    public EnumByte Deserialize_Strict() => JsonSerializer.Deserialize<EnumByte>("\"two\"", _jsoStringCamelCaseStrict);

    public void Test()
    {
        GlobalSetup();

        if (Serialize_Number() != "2") throw new InvalidOperationException("Serialize_Number");
        if (Serialize_String() != "\"two\"") throw new InvalidOperationException("Serialize_String");
        if (Serialize_Strict() != "\"two\"") throw new InvalidOperationException("Serialize_Strict");
        if (Deserialize_Number() != EnumByte.Two) throw new InvalidOperationException("Deserialize_Number");
        if (Deserialize_String() != EnumByte.Two) throw new InvalidOperationException("Deserialize_String");
        if (Deserialize_Strict() != EnumByte.Two) throw new InvalidOperationException("Deserialize_Strict");
    }
}