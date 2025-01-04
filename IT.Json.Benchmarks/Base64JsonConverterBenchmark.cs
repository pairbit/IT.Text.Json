using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Json.Converters;
using System.Buffers;
using System.Text.Json;

namespace IT.Json.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class Base64JsonConverterBenchmark
{
    private static byte[] _data = null!;
    private static byte[] _dataBase64 = null!;
    private static JsonSerializerOptions _jso = null!;

    public int Length { get; set; } = 1024*1024;//1MB

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new MemoryByteJsonConverter());
        _jso.Converters.Add(new ReadOnlyMemoryByteJsonConverter());
        _jso.Converters.Add(new ArrayByteJsonConverter());
        _jso.Converters.Add(new MemoryOwnerByteJsonConverter());

        _data = new byte[Length];
        Random.Shared.NextBytes(_data);

        _dataBase64 = JsonSerializer.SerializeToUtf8Bytes(_data);
    }

    [Benchmark]
    public Memory<byte> Deserialize_Memory_Default() => JsonSerializer.Deserialize<Memory<byte>>(_dataBase64);
    
    [Benchmark]
    public Memory<byte> Deserialize_Memory_IT() => JsonSerializer.Deserialize<Memory<byte>>(_dataBase64, _jso);

    [Benchmark]
    public byte[] Deserialize_Array_Default() => JsonSerializer.Deserialize<byte[]>(_dataBase64)!;

    [Benchmark]
    public byte[] Deserialize_Array_IT() => JsonSerializer.Deserialize<byte[]>(_dataBase64, _jso)!;

    [Benchmark]
    public void Deserialize_Owner_IT()
    {
        using var owner = JsonSerializer.Deserialize<IMemoryOwner<byte>>(_dataBase64, _jso)!;
        if (!owner.Memory.Span.SequenceEqual(_data)) throw new InvalidOperationException("Deserialize_Owner_IT");
    }

    public void Test()
    {
        GlobalSetup();

        if (!Deserialize_Memory_Default().Span.SequenceEqual(_data)) throw new InvalidOperationException("Deserialize_Memory_Default");
        if (!Deserialize_Memory_IT().Span.SequenceEqual(_data)) throw new InvalidOperationException("Deserialize_Memory_IT");
        
        if (!Deserialize_Array_Default().SequenceEqual(_data)) throw new InvalidOperationException("Deserialize_Array_Default");
        if (!Deserialize_Array_IT().SequenceEqual(_data)) throw new InvalidOperationException("Deserialize_Array_IT");

        Deserialize_Owner_IT();
    }
}