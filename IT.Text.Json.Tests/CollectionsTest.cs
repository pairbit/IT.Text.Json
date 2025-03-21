using IT.Buffers;
using IT.Buffers.Extensions;
using IT.Text.Json.Converters;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IT.Text.Json.Tests;

public class CollectionsTest
{
    const int MAX = 70;

    public class RentedEntity : IDisposable
    {
        [JsonConverter(typeof(RentedArraySegmentByteJsonConverter))]
        //[RentedCollectionJsonConverterFactory(70)]
        public ArraySegment<byte> Bytes { get; set; }

        [RentedCollectionJsonConverterFactory(MAX, 32)]
        public ReadOnlySequence<int> Ints { get; set; }

        public void Dispose()
        {
            BufferPool.TryReturn(Bytes);
            Bytes = default;

            var count = BufferPool.TryReturn(Ints);
            Assert.That(count > 0, Is.True);
            Ints = default;
        }
    }

    [Test]
    public void Test()
    {
        var count = MAX;
        var bytes = ArrayPool<byte>.Shared.Rent(count);
        Random.Shared.NextBytes(bytes.AsSpan(0, count));

        var ints = ArrayPool<int>.Shared.Rent(count);
        for (int i = 0; i < count; i++)
        {
            ints[i] = Random.Shared.Next(1000, short.MaxValue);
        }
        var intSeq = ints.AsMemory(0, count).SplitAndRent(16, isRented: true);
        using var rentedEntity = new RentedEntity()
        {
            Bytes = new ArraySegment<byte>(bytes, 0, count),
            Ints = intSeq
        };

        var jso = new JsonSerializerOptions();
        jso.Converters.Add(new CollectionJsonConverterFactory());

        var bin = JsonSerializer.SerializeToUtf8Bytes(rentedEntity, jso);
        var str = System.Text.Encoding.UTF8.GetString(bin);

        using var rentedEntity2 = Json.Deserialize<RentedEntity>(bin, jso)!;

        Assert.That(SequenceEqual(rentedEntity2.Bytes, bytes.AsSpan(0, count)), Is.True);
        Assert.That(SequenceEqual(rentedEntity2.Ints, ints.AsSpan(0, count)), Is.True);
        Assert.That(SequenceEqual(rentedEntity2.Ints, intSeq), Is.True);
    }

    private static bool SequenceEqual<T>(ArraySegment<T> first, ReadOnlySpan<T> second)
    {
        return first.AsSpan().SequenceEqual(second);
    }

    private static bool SequenceEqual<T>(ReadOnlyMemory<T> first, ReadOnlySpan<T> second)
    {
        return first.Span.SequenceEqual(second);
    }

    private static bool SequenceEqual<T>(Memory<T> first, ReadOnlySpan<T> second)
        => SequenceEqual((ReadOnlyMemory<T>)first, second);

    private static bool SequenceEqual<T>(in ReadOnlySequence<T> first, ReadOnlySpan<T> other)
    {
        return first.SequenceEqual(other);
    }

    private static bool SequenceEqual<T>(in ReadOnlySequence<T> first, in ReadOnlySequence<T> other)
    {
        return first.SequenceEqual(other);
    }
}