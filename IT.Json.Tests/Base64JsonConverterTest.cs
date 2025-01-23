using IT.Buffers;
using IT.Buffers.Extensions;
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

        var entityCopy = await DeserializeAsync<EntityByte>(new MemoryStream(bin), _jso);

        var copyData = entityCopy!.Data;

        Assert.That(ReferenceEquals(entityInt.Data.Array, copyData.Array), Is.False);
        Assert.That(entityInt.Data.AsSpan().SequenceEqual(copyData.AsSpan()), Is.True);

        entityCopy.Dispose();

        Assert.ThrowsAsync<JsonException>(async () =>
            await DeserializeAsync<EntityByte>(new MemoryStream(bakInt), _jso));
    }

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));

        if (utf8Json is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var span = arraySegment.AsSpan(checked((int)memoryStream.Position));

            var value = Json.Deserialize<TValue>(span, options);

            memoryStream.Seek(span.Length, SeekOrigin.Current);

            return value;
        }


        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Json.Deserialize<TValue>(memory.Span, options);
            }
            else
            {
                var seq = builder.Build();
                var reader = new Utf8JsonReader(seq);
                return Json.Deserialize<TValue>(ref reader, options);
            }
        }
        finally
        {
            ReadOnlySequenceBuilder<byte>.Pool.Return(builder);
        }
    }
}