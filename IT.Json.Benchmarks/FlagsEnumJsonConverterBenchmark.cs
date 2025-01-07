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

    private static JsonSerializerOptions _jso = null!;
    private static JsonSerializerOptions _jso_IT = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jso_IT = new JsonSerializerOptions();
        _jso_IT.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));

        _maxFlags = (EnumByteFlags)15;
        _maxFlagsString = Serialize_String();
    }

    [Benchmark]
    public byte[] Serialize_Number() => JsonSerializer.SerializeToUtf8Bytes(_maxFlags);

    [Benchmark]
    public byte[] Serialize_String() => JsonSerializer.SerializeToUtf8Bytes(_maxFlags, _jso);

    [Benchmark]
    public byte[] Serialize_IT() => JsonSerializer.SerializeToUtf8Bytes(_maxFlags, _jso_IT);

    [Benchmark]
    public EnumByteFlags Deserialize_Number() => JsonSerializer.Deserialize<EnumByteFlags>("15"u8);

    [Benchmark]
    public EnumByteFlags Deserialize_String() => JsonSerializer.Deserialize<EnumByteFlags>(_maxFlagsString, _jso);

    [Benchmark]
    public EnumByteFlags Deserialize_IT() => JsonSerializer.Deserialize<EnumByteFlags>(_maxFlagsString, _jso_IT);

    public void Test()
    {
        GlobalSetup();

        if (!Serialize_Number().AsSpan().SequenceEqual("15"u8)) throw new InvalidOperationException(nameof(Serialize_Number));
        if (!Serialize_String().AsSpan().SequenceEqual(_maxFlagsString)) throw new InvalidOperationException(nameof(Serialize_String));
        if (!Serialize_IT().AsSpan().SequenceEqual(_maxFlagsString)) throw new InvalidOperationException(nameof(Serialize_IT));

        if (Deserialize_Number() != _maxFlags) throw new InvalidOperationException(nameof(Deserialize_Number));
        if (Deserialize_String() != _maxFlags) throw new InvalidOperationException(nameof(Deserialize_String));
        if (Deserialize_IT() != _maxFlags) throw new InvalidOperationException(nameof(Deserialize_IT));
    }
}