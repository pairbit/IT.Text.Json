using IT.Json.Converters;
using IT.Json.Internal;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Tests;

internal class Base64JsonConverterTest
{
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
        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new Base64JsonConverterFactory());

        var bakInt = Convert.FromBase64String("eyJEYXRhIjoiZlNodyIsIklkIjozMjc2N30=");
        using var entityInt = JsonSerializer.Deserialize<EntityInt>(bakInt, jso)!;

        //var array = new byte[3];
        //Random.Shared.NextBytes(array);

        var entity = new EntityByte() { Id = 1, Data = entityInt.Data };

        var bin = JsonSerializer.SerializeToUtf8Bytes(entity, jso);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        EntityByte? entityCopy;
        ArrayPoolShared<byte>.AddToList();
        try
        {
            entityCopy = JsonSerializer.Deserialize<EntityByte>(bakInt, jso);
        }
        catch (JsonException ex)
        {
            ArrayPoolShared<byte>.ReturnAndClear();
            throw;
        }

        var data = entityCopy!.Data;

        Assert.That(entity.Data.AsSpan().SequenceEqual(data.AsSpan()));

        entityCopy.Dispose();
    }

    //[Test]
    //public async Task TestAsync()
    //{
    //    await JsonSerializer.DeserializeAsync(inputStream, context.ModelType, SerializerOptions);
    //}
}