using IT.Json.Converters;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Json.Tests;

public class CollectionsTest
{
    public class RentedEntity : IDisposable
    {
        [JsonConverter(typeof(RentedArraySegmentByteJsonConverter))]
        public ArraySegment<byte> Bytes { get; set; }

        [RentedCollectionJsonConverterFactory(40)]
        public ArraySegment<int> Ints { get; set; }

        public void Dispose()
        {
            var bytesArray = Bytes.Array;
            if (bytesArray != null && bytesArray.Length > 0)
            {
                Bytes = default;
                ArrayPool<byte>.Shared.Return(bytesArray);
            }

            var intsArray = Ints.Array;
            if (intsArray != null && intsArray.Length > 0)
            {
                Ints = default;
                ArrayPool<int>.Shared.Return(intsArray);
            }
        }
    }

    [Test]
    public void Test()
    {
        var count = 40;
        var bytes = ArrayPool<byte>.Shared.Rent(count);
        Random.Shared.NextBytes(bytes.AsSpan(0, count));

        var ints = ArrayPool<int>.Shared.Rent(count);
        for (int i = 0; i < count; i++)
        {
            ints[i] = Random.Shared.Next(1000, short.MaxValue);
        }

        var rentedEntity = new RentedEntity()
        {
            Bytes = new ArraySegment<byte>(bytes, 0, count),
            Ints = new ArraySegment<int>(ints, 0, count)
        };

        var bin = JsonSerializer.SerializeToUtf8Bytes(rentedEntity);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        using var rentedEntity2 = Json.Deserialize<RentedEntity>(bin)!;

        Assert.That(rentedEntity2.Bytes.AsSpan().SequenceEqual(bytes.AsSpan(0, count)), Is.True);
        Assert.That(rentedEntity2.Ints.AsSpan().SequenceEqual(ints.AsSpan(0, count)), Is.True);
    }
}