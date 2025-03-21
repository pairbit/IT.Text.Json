using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Buffers;
using IT.Text.Json.Converters;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class SequenceEnumJsonConverterBenchmark
{
    private static EnumByte _enumValue;
    private static ReadOnlySequence<byte> _enumString;

    private static JsonSerializerOptions _jso = null!;
    private static JsonSerializerOptions _jso_IT = null!;

    [Params(1, 2, 4)]
    public int Segments { get; set; } = 2;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jso_IT = new JsonSerializerOptions();
        _jso_IT.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));

        _enumValue = EnumByte.TwoTwoTwo;
        var enumString = JsonSerializer.SerializeToUtf8Bytes(_enumValue, _jso);

        _enumString = new ReadOnlySequenceBuilder<byte>().Add(enumString, Segments).Build();
    }

    [Benchmark]
    public EnumByte Deserialize_String() => Deserialize<EnumByte>(_enumString, _jso);

    [Benchmark]
    public EnumByte Deserialize_IT() => Deserialize<EnumByte>(_enumString, _jso_IT);

    public void Test()
    {
        GlobalSetup();

        if (Deserialize_String() != _enumValue) throw new InvalidOperationException(nameof(Deserialize_String));
        if (Deserialize_IT() != _enumValue) throw new InvalidOperationException(nameof(Deserialize_IT));
    }

    private static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        var utf8Reader = new Utf8JsonReader(utf8Json);
        return JsonSerializer.Deserialize<TValue>(ref utf8Reader, options);
    }
}