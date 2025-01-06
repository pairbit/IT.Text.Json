using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Buffers;
using IT.Buffers.Pool;
using IT.Json.Converters;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class SequenceEnumJsonConverterBenchmark
{
    private static ReadOnlySequence<byte> _enumString;
    private static ReadOnlySequenceBuilder<byte> _sequenceBuilder = null!;

    private static JsonSerializerOptions _jso = null!;
    private static JsonSerializerOptions _jso_Strict = null!;

    public int Segments { get; set; } = 2;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jso_Strict = new JsonSerializerOptions();
        _jso_Strict.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));

        var enumString = JsonSerializer.SerializeToUtf8Bytes(EnumByte.Two, _jso);

        _sequenceBuilder = ReadOnlySequenceBuilderPool<byte>.Rent(Segments);

        _enumString = _sequenceBuilder.Add(enumString, Segments).Build();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        ReadOnlySequenceBuilderPool<byte>.Return(_sequenceBuilder);
    }

    [Benchmark]
    public EnumByte Deserialize_String() => Deserialize<EnumByte>(_enumString, _jso);

    [Benchmark]
    public EnumByte Deserialize_Strict() => Deserialize<EnumByte>(_enumString, _jso_Strict);

    public void Test()
    {
        GlobalSetup();

        if (Deserialize_String() != EnumByte.Two) throw new InvalidOperationException("Deserialize_String");
        if (Deserialize_Strict() != EnumByte.Two) throw new InvalidOperationException("Deserialize_Strict");
    }

    private static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        var utf8Reader = new Utf8JsonReader(utf8Json);
        return JsonSerializer.Deserialize<TValue>(ref utf8Reader, options);
    }
}