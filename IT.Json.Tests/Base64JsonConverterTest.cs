using IT.Json.Converters;
using IT.Json.Internal;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Tests;

internal class Base64JsonConverterTest
{
    private static readonly JsonSerializerOptions _jso;

    static Base64JsonConverterTest()
    {
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new Base64JsonConverterFactory());

        _jso = jso;
    }

    public class EntityInt : IDisposable
    {
        [JsonConverter(typeof(RentedArraySegmentByteJsonConverter))]
        public ArraySegment<byte> Data { get; set; }

        public int Id { get; set; }

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

    public class EntityByte : IDisposable
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

    [Test]
    public void Test()
    {
        var bakInt = Convert.FromBase64String("eyJEYXRhIjoiZlNodyIsIklkIjozMjc2N30=");
        using var entityInt = JsonSerializer.Deserialize<EntityInt>(bakInt, _jso)!;

        //var array = new byte[3];
        //Random.Shared.NextBytes(array);

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        Test(entityInt.Data, bin);
        Test(entityInt.Data, bakInt);
    }

    [Test]
    public async Task TestAsync()
    {
        var bakInt = Convert.FromBase64String("eyJEYXRhIjoiZlNodyIsIklkIjozMjc2N30=");
        using var entityInt = JsonSerializer.Deserialize<EntityInt>(bakInt, _jso)!;

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        await TestAsync(entityInt.Data, bin);
        await TestAsync(entityInt.Data, bakInt);
    }

    private void Test(ArraySegment<byte> data, byte[] bak)
    {
        EntityByte? entityCopy;
        ArrayPoolShared<byte>.AddToList();
        try
        {
            entityCopy = JsonSerializer.Deserialize<EntityByte>(bak, _jso);
            ArrayPoolShared<byte>.Clear();
        }
        catch (JsonException)
        {
            ArrayPoolShared<byte>.ReturnAndClear();

            return;
        }

        var copyData = entityCopy!.Data;
        
        Assert.That(ReferenceEquals(data.Array, copyData.Array), Is.False);
        Assert.That(data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        entityCopy.Dispose();
    }

    private async Task TestAsync(ArraySegment<byte> data, byte[] bak)
    {
        EntityByte? entityCopy;

        var rentedList = new RentedList();
        var jso = new JsonSerializerOptions(_jso);
        jso.Converters.Add(rentedList);

        try
        {
            var stream = new MemoryStream(bak);
            entityCopy = await JsonSerializer.DeserializeAsync<EntityByte>(stream, jso);
            rentedList.Clear();
        }
        catch (JsonException)
        {
            rentedList.ReturnAndClear();
            return;
        }

        var copyData = entityCopy!.Data;

        Assert.That(ReferenceEquals(data.Array, copyData.Array), Is.False);
        Assert.That(data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        entityCopy.Dispose();
    }
}