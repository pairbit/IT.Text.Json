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
    TwoTwoTwo = 222
}

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class EnumJsonConverterBenchmark
{
    private static JsonSerializerOptions _jso = null!;
    private static JsonSerializerOptions _jso_IT = null!;
    private static EnumByte _enumValue;
    private static byte[] _enumNumber = null!;
    private static byte[] _enumString = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jso_IT = new JsonSerializerOptions();
        _jso_IT.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));

        _enumValue = EnumByte.TwoTwoTwo;
        _enumNumber = Serialize_Number();
        _enumString = Serialize_String();
    }

    [Benchmark]
    public byte[] Serialize_Number() => JsonSerializer.SerializeToUtf8Bytes(_enumValue);

    [Benchmark]
    public byte[] Serialize_String() => JsonSerializer.SerializeToUtf8Bytes(_enumValue, _jso);

    [Benchmark]
    public byte[] Serialize_IT() => JsonSerializer.SerializeToUtf8Bytes(_enumValue, _jso_IT);

    [Benchmark]
    public EnumByte Deserialize_Number() => JsonSerializer.Deserialize<EnumByte>(_enumNumber);

    [Benchmark]
    public EnumByte Deserialize_String() => JsonSerializer.Deserialize<EnumByte>(_enumString, _jso);

    [Benchmark]
    public EnumByte Deserialize_IT() => JsonSerializer.Deserialize<EnumByte>(_enumString, _jso_IT);

    public void Test()
    {
        GlobalSetup();

        if (!Serialize_Number().AsSpan().SequenceEqual(_enumNumber)) throw new InvalidOperationException(nameof(Serialize_Number));
        if (!Serialize_String().AsSpan().SequenceEqual(_enumString)) throw new InvalidOperationException(nameof(Serialize_String));
        if (!Serialize_IT().AsSpan().SequenceEqual(_enumString)) throw new InvalidOperationException(nameof(Serialize_IT));
        if (Deserialize_Number() != _enumValue) throw new InvalidOperationException(nameof(Deserialize_Number));
        if (Deserialize_String() != _enumValue) throw new InvalidOperationException(nameof(Deserialize_String));
        if (Deserialize_IT() != _enumValue) throw new InvalidOperationException(nameof(Deserialize_IT));
    }
}