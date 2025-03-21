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
        jso.Converters.Add(new Base64JsonConverterFactory(int.MaxValue, (byte)'!'));

        _jso = jso;
    }

    public class EntityInt : IDisposable
    {
        [RentedBase64JsonConverterFactory(int.MaxValue, (byte)'!')]
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
                try
                {
                    ArrayPool<byte>.Shared.Return(dataArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }

    public class EntityByte : IDisposable
    {
        [RentedBase64JsonConverterFactory(int.MaxValue, (byte)'!')]
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
                try
                {
                    ArrayPool<byte>.Shared.Return(dataArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }

    [Test]
    public Task TestEmpty() => Test("{\"Data\":\"\",\"Id\":32767}"u8.ToArray());

    [Test]
    public Task Test() => Test("{\"Data\":\"cXdlcnR5\",\"Id\":32767}"u8.ToArray());

    [Test]
    public Task TestRaw() => Test("{\"Data\":\"!qwerty\",\"Id\":32767}"u8.ToArray());

    private static async Task Test(byte[] bakInt)
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

        //async
        using var entityCopy2 = await Json.DeserializeAsync<EntityByte>(new MemoryStream(bin), _jso);

        var copyData2 = entityCopy2!.Data;

        if (copyData2.Count > 0)
            Assert.That(ReferenceEquals(entityInt.Data.Array, copyData2.Array), Is.False);

        Assert.That(entityInt.Data.AsSpan().SequenceEqual(copyData2.AsSpan()), Is.True);

        Assert.ThrowsAsync<JsonException>(async () =>
            await Json.DeserializeAsync<EntityByte>(new MemoryStream(bakInt), _jso));
    }
}