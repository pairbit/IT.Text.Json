using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Text.Json;

namespace IT.Json.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class RentedArrayBase64JsonConverterBenchmark
{
    private static byte[] _data = null!;
    private static byte[] _dataBase64 = null!;
    private static byte[] _invalidDataBase64 = null!;

    [Params(1024, 80 * 1024, 1024 * 1024)]
    public int Length { get; set; } = 80 * 1024;//1MB
    
    [Params(10)]
    public int Count { get; set; } = 10;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _data = new byte[Length];
        Random.Shared.NextBytes(_data);

        var rentedDatas = new RentedData[Count];
        for (int i = 0; i < rentedDatas.Length; i++)
        {
            rentedDatas[i] = new RentedData() { Data = _data, Id = 255 };
        }

        _dataBase64 = JsonSerializer.SerializeToUtf8Bytes(rentedDatas);

        _invalidDataBase64 = new byte[_dataBase64.Length];
        _dataBase64.CopyTo(_invalidDataBase64.AsSpan());

        _invalidDataBase64[^3] = 54;//invalid Id: 256

        //var str = Encoding.UTF8.GetString(_invalidDataBase64);
    }

    [Benchmark]
    public void Deserialize_Default()
    {
        var rentedDatas = JsonSerializer.Deserialize<RentedData[]>(_dataBase64)!;

        foreach (var rentedData in rentedDatas!)
        {
            if (!rentedData.Data.AsSpan().SequenceEqual(_data))
                throw new InvalidOperationException(nameof(Deserialize_Default));

            rentedData.Dispose();
        }
    }

    [Benchmark]
    public bool Deserialize_Ex_Default()
    {
        try
        {
            JsonSerializer.Deserialize<RentedData[]>(_invalidDataBase64);
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
        var rentedDatas = await JsonSerializer.DeserializeAsync<RentedData[]>(new MemoryStream(_dataBase64));

        foreach (var rentedData in rentedDatas!)
        {
            if (!rentedData!.Data.AsSpan().SequenceEqual(_data))
                throw new InvalidOperationException(nameof(Deserialize_Stream_Default));

            rentedData.Dispose();
        }
    }

    [Benchmark]
    public async Task<bool> Deserialize_Stream_Ex_Default()
    {
        try
        {
            await JsonSerializer.DeserializeAsync<RentedData[]>(new MemoryStream(_invalidDataBase64));
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
        var rentedDatas = Json.Deserialize<RentedData[]>(_dataBase64)!;

        foreach (var rentedData in rentedDatas)
        {
            if (!rentedData.Data.AsSpan().SequenceEqual(_data))
                throw new InvalidOperationException(nameof(Deserialize_IT));

            rentedData.Dispose();
        }
    }

    [Benchmark]
    public bool Deserialize_Ex_IT()
    {
        try
        {
            Json.Deserialize<RentedData[]>(_invalidDataBase64);
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
        var rentedDatas = await Json.DeserializeAsync<RentedData[]>(new MemoryStream(_dataBase64));
        
        foreach (var rentedData in rentedDatas!)
        {
            if (!rentedData!.Data.AsSpan().SequenceEqual(_data))
                throw new InvalidOperationException(nameof(Deserialize_Stream_IT));

            rentedData.Dispose();
        }
    }

    [Benchmark]
    public async Task<bool> Deserialize_Stream_Ex_IT()
    {
        try
        {
            await Json.DeserializeAsync<RentedData[]>(new MemoryStream(_invalidDataBase64));
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