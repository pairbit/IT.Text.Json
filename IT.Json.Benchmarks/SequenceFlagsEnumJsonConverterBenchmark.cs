using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Buffers;
using IT.Buffers.Pool;
using IT.Json.Converters;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class SequenceFlagsEnumJsonConverterBenchmark
{
    private static EnumByteFlags _maxFlags;
    private static ReadOnlySequence<byte> _maxFlagsSequence;
    private static ReadOnlySequenceBuilder<byte> _sequenceBuilder = null!;

    private static JsonSerializerOptions _jso = null!;
    private static JsonSerializerOptions _jso_Strict = null!;

    [Params(1, 2, 4, 8, 10)]
    public int Segments { get; set; } = 2;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jso_Strict = new JsonSerializerOptions();
        _jso_Strict.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));

        _maxFlags = (EnumByteFlags)15;
        var maxFlags = JsonSerializer.SerializeToUtf8Bytes(_maxFlags, _jso);
        var maxFlagsString = Encoding.UTF8.GetString(maxFlags);

        _sequenceBuilder = ReadOnlySequenceBuilderPool<byte>.Rent(Segments);

        _maxFlagsSequence = _sequenceBuilder.Add(maxFlags, Segments).Build();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        ReadOnlySequenceBuilderPool<byte>.Return(_sequenceBuilder);
    }

    [Benchmark]
    public EnumByteFlags Deserialize_String() => Deserialize<EnumByteFlags>(_maxFlagsSequence, _jso);

    [Benchmark]
    public EnumByteFlags Deserialize_Strict() => Deserialize<EnumByteFlags>(_maxFlagsSequence, _jso_Strict);

    public void Test()
    {
        GlobalSetup();

        if (Deserialize_String() != _maxFlags) throw new InvalidOperationException("Deserialize_String");
        if (Deserialize_Strict() != _maxFlags) throw new InvalidOperationException("Deserialize_Strict");
    }

    private static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        var utf8Reader = new Utf8JsonReader(utf8Json);
        return JsonSerializer.Deserialize<TValue>(ref utf8Reader, options);
    }
}