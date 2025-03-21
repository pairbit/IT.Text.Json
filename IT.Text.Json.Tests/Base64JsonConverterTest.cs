using IT.Buffers.Extensions;
using IT.Text.Json.Converters;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Tests;

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
        
        [JsonIgnore]
        public string DataString => Encoding.UTF8.GetString(Data);

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

        [JsonIgnore]
        public string DataString => Encoding.UTF8.GetString(Data);

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
    public void TestEmpty() => Test("{\"Data\":\"\",\"Id\":32767}"u8.ToArray());

    [Test]
    public Task TestEmptyAsync() => TestAsync("{\"Data\":\"\",\"Id\":32767}"u8.ToArray());

    [Test]
    public void Test() => Test("{\"Data\":\"cXdlcnR5\",\"Id\":32767}"u8.ToArray());

    [Test]
    public Task TestAsync() => TestAsync("{\"Data\":\"cXdlcnR5\",\"Id\":32767}"u8.ToArray());

    private static void Test(byte[] bakInt)
    {
        using var entityInt = Json.Deserialize<EntityInt>(bakInt, _jso)!;

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = Encoding.UTF8.GetString(bin);

        using var entityCopy = Json.Deserialize<EntityByte>(bin, _jso);

        var copyData = entityCopy!.Data;

        if (copyData.Count > 0)
            Assert.That(ReferenceEquals(entityInt.Data.Array, copyData.Array), Is.False);

        Assert.That(entityInt.Data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        Assert.Throws<JsonException>(() => Json.Deserialize<EntityByte>(bakInt, _jso));
    }

    private static async Task TestAsync(byte[] bakInt)
    {
        using var entityInt = Json.Deserialize<EntityInt>(bakInt, _jso)!;

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = Encoding.UTF8.GetString(bin);

        var entityCopy = await Json.DeserializeAsync<EntityByte>(new MemoryStream(bin), _jso);

        var copyData = entityCopy!.Data;

        if (copyData.Count > 0)
            Assert.That(ReferenceEquals(entityInt.Data.Array, copyData.Array), Is.False);
        Assert.That(entityInt.Data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        entityCopy.Dispose();

        Assert.ThrowsAsync<JsonException>(async () =>
            await Json.DeserializeAsync<EntityByte>(new MemoryStream(bakInt), _jso));
    }
}