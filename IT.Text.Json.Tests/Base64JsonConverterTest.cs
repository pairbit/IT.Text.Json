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
        //[RentedBase64JsonConverterFactory(int.MaxValue, (byte)'!')]
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
        //[RentedBase64JsonConverterFactory(int.MaxValue, (byte)'!')]
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
    public Task EmptyTest() => Test("{\"Data\":\"\",\"Id\":32767}"u8.ToArray(), []);

    [Test]
    public Task Base64Test() => Test("{\"Data\":\"cXdlcnR5\",\"Id\":32767}"u8.ToArray(), "qwerty"u8.ToArray());

    [Test]
    public Task RawTest() => Test("{\"Data\":\"!qwerty\",\"Id\":32767}"u8.ToArray(), "qwerty"u8.ToArray());

    [Test]
    public Task EscapedRawTest() => Test("{\"Data\":\"!\\\"qwerty\\\"\",\"Id\":32767}"u8.ToArray(), "\"qwerty\""u8.ToArray());

    private static async Task Test(byte[] entityIntUtf8, byte[] dataValid)
    {
        using var entityInt = Json.Deserialize<EntityInt>(entityIntUtf8, _jso)!;

        var data = entityInt.Data;
        Assert.That(data.AsSpan().SequenceEqual(dataValid), Is.True);

        var entity = new EntityByte() { Id = 1, Data = data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, _jso);
        var str = Encoding.UTF8.GetString(bin);

        using var entityByte = Json.Deserialize<EntityByte>(bin, _jso);

        var copyData = entityByte!.Data;

        if (copyData.Count > 0)
            Assert.That(ReferenceEquals(data.Array, copyData.Array), Is.False);

        Assert.That(data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        Assert.Throws<JsonException>(() => Json.Deserialize<EntityByte>(entityIntUtf8, _jso));

        //async
        using var entityByte2 = await Json.DeserializeAsync<EntityByte>(new MemoryStream(bin), _jso);

        var copyData2 = entityByte2!.Data;

        if (copyData2.Count > 0)
            Assert.That(ReferenceEquals(data.Array, copyData2.Array), Is.False);

        Assert.That(data.AsSpan().SequenceEqual(copyData2.AsSpan()), Is.True);

        Assert.ThrowsAsync<JsonException>(async () =>
            await Json.DeserializeAsync<EntityByte>(new MemoryStream(entityIntUtf8), _jso));
    }
}