using IT.Buffers;
using IT.Buffers.Extensions;
using System.Text.Json;

namespace IT.Json.Benchmarks;

internal static class xJson
{
    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));

        //if (utf8Json is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    var span = arraySegment.AsSpan(checked((int)memoryStream.Position));

        //    var value = Json.Deserialize<TValue>(span, options);

        //    memoryStream.Seek(span.Length, SeekOrigin.Current);

        //    return value;
        //}


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