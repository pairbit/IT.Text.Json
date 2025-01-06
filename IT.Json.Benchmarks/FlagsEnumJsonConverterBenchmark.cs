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
    Four = 4,
    Eight = 8,
}

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class FlagsEnumJsonConverterBenchmark
{
    private static EnumByteFlags _maxFlags;
    private static byte[] _maxFlagsString = null!;

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

        _maxFlags = (EnumByteFlags)15;
        _maxFlagsString = Serialize_String();
    }

    [Benchmark]
    public byte[] Serialize_Number() => JsonSerializer.SerializeToUtf8Bytes(_maxFlags);

    [Benchmark]
    public byte[] Serialize_String() => JsonSerializer.SerializeToUtf8Bytes(_maxFlags, _jsoStringCamelCase);

    [Benchmark]
    public byte[] Serialize_Strict() => JsonSerializer.SerializeToUtf8Bytes(_maxFlags, _jsoStringCamelCaseStrict);

    [Benchmark]
    public EnumByteFlags Deserialize_Number() => JsonSerializer.Deserialize<EnumByteFlags>("15"u8);

    [Benchmark]
    public EnumByteFlags Deserialize_String() => JsonSerializer.Deserialize<EnumByteFlags>(_maxFlagsString, _jsoStringCamelCase);

    [Benchmark]
    public EnumByteFlags Deserialize_Strict() => JsonSerializer.Deserialize<EnumByteFlags>(_maxFlagsString, _jsoStringCamelCaseStrict);

    public void Test()
    {
        GlobalSetup();

        if (!Serialize_Number().AsSpan().SequenceEqual("15"u8)) throw new InvalidOperationException("Serialize_Number");
        if (!Serialize_String().AsSpan().SequenceEqual(_maxFlagsString)) throw new InvalidOperationException("Serialize_String");
        if (!Serialize_Strict().AsSpan().SequenceEqual(_maxFlagsString)) throw new InvalidOperationException("Serialize_Strict");

        if (Deserialize_Number() != _maxFlags) throw new InvalidOperationException("Deserialize_Number");
        if (Deserialize_String() != _maxFlags) throw new InvalidOperationException("Deserialize_String");
        if (Deserialize_Strict() != _maxFlags) throw new InvalidOperationException("Deserialize_Strict");
    }
}