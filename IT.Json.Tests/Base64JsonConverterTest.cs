using IT.Json.Converters;
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
        using var entityInt = Json.Deserialize<EntityInt>(bakInt, _jso)!;

        //var array = new byte[3];
        //Random.Shared.NextBytes(array);

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        using var entityCopy = Json.Deserialize<EntityByte>(bin, _jso);

        var copyData = entityCopy!.Data;

        Assert.That(ReferenceEquals(entityInt.Data.Array, copyData.Array), Is.False);
        Assert.That(entityInt.Data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        Assert.Throws<JsonException>(() => Json.Deserialize<EntityByte>(bakInt, _jso));
    }

    [Test]
    public async Task TestAsync()
    {
        var bakInt = Convert.FromBase64String("eyJEYXRhIjoiZlNodyIsIklkIjozMjc2N30=");
        using var entityInt = Json.Deserialize<EntityInt>(bakInt, _jso)!;

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        var entityCopy = await Json.DeserializeAsync<EntityByte>(new MemoryStream(bin), _jso);

        var copyData = entityCopy!.Data;

        Assert.That(ReferenceEquals(entityInt.Data.Array, copyData.Array), Is.False);
        Assert.That(entityInt.Data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        entityCopy.Dispose();

        Assert.ThrowsAsync<JsonException>(async () =>
            await Json.DeserializeAsync<EntityByte>(new MemoryStream(bakInt), _jso));
    }
}