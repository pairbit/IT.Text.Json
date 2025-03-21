using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using IT.Text.Json.Converters;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Benchmarks;

public class RentedData : IDisposable
{
    [JsonConverter(typeof(RentedArraySegmentByteJsonConverter))]
    public ArraySegment<byte> Data { get; set; }

    public byte Id { get; set; }

    public void Dispose()
    {
        var dataArray = Data.Array;
        if (dataArray != null && dataArray.Length > 0)
        {
            Data = default;
            ArrayPool<byte>.Shared.Return(dataArray);
        }
    }
}

public class RentedDataInt
{
    [JsonConverter(typeof(RentedArraySegmentByteJsonConverter))]
    public ArraySegment<byte> Data { get; set; }

    public int Id { get; set; }
}

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class RentedBase64JsonConverterBenchmark
{
    private static byte[] _data = null!;
    private static byte[] _dataBase64 = null!;
    private static byte[] _invalidDataBase64 = null!;

    [Params(1024, 80 * 1024, 1024 * 1024, 16 * 1024 * 1024)]
    public int Length { get; set; } = 16 * 1024 * 1024;//1MB

    [GlobalSetup]
    public void GlobalSetup()
    {
        _data = new byte[Length];
        Random.Shared.NextBytes(_data);

        _dataBase64 = JsonSerializer.SerializeToUtf8Bytes(new RentedData() { Data = _data, Id = 255 });
        _invalidDataBase64 = JsonSerializer.SerializeToUtf8Bytes(new RentedDataInt() { Data = _data, Id = 256 });
    }

    [Benchmark]
    public void Deserialize_Default()
    {
        using var rentedData = JsonSerializer.Deserialize<RentedData>(_dataBase64)!;
        if (!rentedData.Data.AsSpan().SequenceEqual(_data))
            throw new InvalidOperationException(nameof(Deserialize_Default));
    }

    [Benchmark]
    public bool Deserialize_Ex_Default()
    {
        try
        {
            JsonSerializer.Deserialize<RentedData>(_invalidDataBase64);
        }
        catch (JsonException)
        {
            return true;
        }
        throw new InvalidOperationException(nameof(Deserialize_Ex_Default));
    }

    [Benchmark]
    public async Task Deserialize_Stream_Default()
    {
        using var rentedData = await JsonSerializer.DeserializeAsync<RentedData>(new MemoryStream(_dataBase64));
        if (!rentedData!.Data.AsSpan().SequenceEqual(_data))
            throw new InvalidOperationException(nameof(Deserialize_Stream_Default));
    }

    [Benchmark]
    public async Task<bool> Deserialize_Stream_Ex_Default()
    {
        try
        {
            await JsonSerializer.DeserializeAsync<RentedData>(new MemoryStream(_invalidDataBase64));
        }
        catch (JsonException)
        {
            return true;
        }
        throw new InvalidOperationException(nameof(Deserialize_Stream_Ex_Default));
    }

    [Benchmark]
    public void Deserialize_IT()
    {
        using var rentedData = Json.Deserialize<RentedData>(_dataBase64)!;
        if (!rentedData.Data.AsSpan().SequenceEqual(_data))
            throw new InvalidOperationException(nameof(Deserialize_IT));
    }

    [Benchmark]
    public bool Deserialize_Ex_IT()
    {
        try
        {
            Json.Deserialize<RentedData>(_invalidDataBase64);
        }
        catch (JsonException)
        {
            return true;
        }
        throw new InvalidOperationException(nameof(Deserialize_Ex_IT));
    }

    [Benchmark]
    public async Task Deserialize_Stream_IT()
    {
        using var rentedData = await Json.DeserializeAsync<RentedData>(new MemoryStream(_dataBase64));
        if (!rentedData!.Data.AsSpan().SequenceEqual(_data))
            throw new InvalidOperationException(nameof(Deserialize_Stream_IT));
    }

    [Benchmark]
    public async Task<bool> Deserialize_Stream_Ex_IT()
    {
        try
        {
            await Json.DeserializeAsync<RentedData>(new MemoryStream(_invalidDataBase64));
        }
        catch (JsonException)
        {
            return true;
        }
        throw new InvalidOperationException(nameof(Deserialize_Stream_Ex_IT));
    }

    public async Task Test()
    {
        GlobalSetup();

        Deserialize_Default();
        Deserialize_IT();

        Deserialize_Ex_Default();
        Deserialize_Ex_IT();

        await Deserialize_Stream_Default();
        await Deserialize_Stream_Ex_Default();
        await Deserialize_Stream_IT();
        await Deserialize_Stream_Ex_IT();
    }
}