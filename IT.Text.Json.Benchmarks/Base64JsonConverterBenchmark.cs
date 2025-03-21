﻿using BenchmarkDotNet.Attributes;
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

    [Params(1024, 1024 * 1024)]
    public int Length { get; set; } = 10;//1MB

    [GlobalSetup]
    public void GlobalSetup()
    {
        _jso = new JsonSerializerOptions();
        _jso.Converters.Add(new Base64JsonConverterFactory());

        _data = new byte[Length];
        Random.Shared.NextBytes(_data);

        _dataBase64 = Serialize_Default();
    }

    [Benchmark]
    public byte[] Serialize_Default() => JsonSerializer.SerializeToUtf8Bytes(_data);

    [Benchmark]
    public byte[] Serialize_IT() => JsonSerializer.SerializeToUtf8Bytes(_data, _jso);

    //[Benchmark]
    //public byte[] Serialize_Default_File() => SerializeToFile(_data, "default");

    //[Benchmark]
    //public byte[] Serialize_IT_File() => SerializeToFile(_data, "it", _jso);

    //[Benchmark]
    public Memory<byte> Deserialize_Memory_Default() => JsonSerializer.Deserialize<Memory<byte>>(_dataBase64);

    //[Benchmark]
    public Memory<byte> Deserialize_Memory_IT() => JsonSerializer.Deserialize<Memory<byte>>(_dataBase64, _jso);

    //[Benchmark]
    public byte[] Deserialize_Array_Default() => JsonSerializer.Deserialize<byte[]>(_dataBase64)!;

    //[Benchmark]
    public byte[] Deserialize_Array_IT() => JsonSerializer.Deserialize<byte[]>(_dataBase64, _jso)!;

    //[Benchmark]
    public void Deserialize_Owner_IT()
    {
        using var owner = JsonSerializer.Deserialize<IMemoryOwner<byte>>(_dataBase64, _jso)!;
        if (!owner.Memory.Span.SequenceEqual(_data)) throw new InvalidOperationException(nameof(Deserialize_Owner_IT));
    }

    public void Test()
    {
        GlobalSetup();

        if (!Serialize_IT().AsSpan().SequenceEqual(_dataBase64)) throw new InvalidOperationException(nameof(Serialize_IT));
        //if (!Serialize_IT_File().AsSpan().SequenceEqual(_dataBase64)) throw new InvalidOperationException(nameof(Serialize_IT_File));
        //if (!Serialize_Default_File().AsSpan().SequenceEqual(_dataBase64)) throw new InvalidOperationException(nameof(Serialize_Default_File));

        if (!Deserialize_Memory_Default().Span.SequenceEqual(_data)) throw new InvalidOperationException(nameof(Deserialize_Memory_Default));
        if (!Deserialize_Memory_IT().Span.SequenceEqual(_data)) throw new InvalidOperationException(nameof(Deserialize_Memory_IT));

        if (!Deserialize_Array_Default().SequenceEqual(_data)) throw new InvalidOperationException(nameof(Deserialize_Array_Default));
        if (!Deserialize_Array_IT().SequenceEqual(_data)) throw new InvalidOperationException(nameof(Deserialize_Array_IT));

        Deserialize_Owner_IT();
    }

    //public static byte[] SerializeToFile<TValue>(TValue value, string filename, JsonSerializerOptions? options = null)
    //{
    //    var file = $@"S:\git\pairbit\IT.Json\stream\{filename}.json";

    //    using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
    //    using var writer = new Utf8JsonWriter(stream);
    //    JsonSerializer.Serialize(writer, value, options);
        
    //    return File.ReadAllBytes(file);
    //}
}