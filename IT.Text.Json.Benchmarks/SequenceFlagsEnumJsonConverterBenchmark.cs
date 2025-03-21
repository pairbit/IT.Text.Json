﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Buffers;
using IT.Json.Converters;
using System.Buffers;
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

    private static JsonSerializerOptions _jso = null!;
    private static JsonSerializerOptions _jso_IT = null!;

    [Params(1, 2, 4, 8, 10)]
    public int Segments { get; set; } = 2;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));

        _jso_IT = new JsonSerializerOptions();
        _jso_IT.Converters.Add(new EnumJsonConverterFactory(JsonNamingPolicy.CamelCase));

        _maxFlags = (EnumByteFlags)15;
        var maxFlags = JsonSerializer.SerializeToUtf8Bytes(_maxFlags, _jso);
        //var maxFlagsString = Encoding.UTF8.GetString(maxFlags);

        _maxFlagsSequence = new ReadOnlySequenceBuilder<byte>().Add(maxFlags, Segments).Build();
    }

    [Benchmark]
    public EnumByteFlags Deserialize_String() => Deserialize<EnumByteFlags>(_maxFlagsSequence, _jso);

    [Benchmark]
    public EnumByteFlags Deserialize_IT() => Deserialize<EnumByteFlags>(_maxFlagsSequence, _jso_IT);

    public void Test()
    {
        GlobalSetup();

        if (Deserialize_String() != _maxFlags) throw new InvalidOperationException(nameof(Deserialize_String));
        if (Deserialize_IT() != _maxFlags) throw new InvalidOperationException(nameof(Deserialize_IT));
    }

    private static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        var utf8Reader = new Utf8JsonReader(utf8Json);
        return JsonSerializer.Deserialize<TValue>(ref utf8Reader, options);
    }
}