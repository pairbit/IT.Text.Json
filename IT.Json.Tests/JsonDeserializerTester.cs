using IT.Buffers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace IT.Json.Tests;

public class JsonDeserializerTester
{
    private const int SegmentsMax = 4;

    public static TValue? Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json, JsonSerializerOptions? options = null)
    {
        return Deserialize<TValue>(Encoding.UTF8.GetBytes(json), options);
    }

    public static TValue? Deserialize<TValue>(ReadOnlyMemory<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        TValue? value = default;
        JsonException? exception = null;
        try
        {
            value = JsonSerializer.Deserialize<TValue>(utf8Json.Span, options);
        }
        catch (JsonException ex)
        {
            exception = ex;
        }

        try
        {
            var value2 = Deserialize<TValue>(new ReadOnlySequence<byte>(utf8Json), options);
            if (!EqualityComparer<TValue>.Default.Equals(value, value2))
                throw new InvalidOperationException("Sequence Single Segment not Equals");
        }
        catch (JsonException ex)
        {
            if (exception!.Message != ex.Message)
                throw new InvalidOperationException("Exception not Equals");
        }

        if (utf8Json.Length > 1)
            DeserializeSegments(value, exception, utf8Json, options);

        return exception != null ? throw exception : value;
    }

    private static void DeserializeSegments<TValue>(TValue value, JsonException? exception, ReadOnlyMemory<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        var builder = ReadOnlySequenceBuilderPool<byte>.Rent(SegmentsMax);

        try
        {
            var max = utf8Json.Length < SegmentsMax ? utf8Json.Length : SegmentsMax;
            for (int i = 2; i <= max; i++)
            {
                try
                {
                    var val = Deserialize<TValue>(SplitToSegments(i, builder, utf8Json), options);

                    if (!EqualityComparer<TValue>.Default.Equals(value, val))
                        throw new InvalidOperationException("Sequence Single Segment not Equals");
                }
                catch (JsonException ex)
                {
                    if (exception == null) throw;

                    if (exception.Message != ex.Message)
                        throw new InvalidOperationException("Exception not Equals");
                }

                builder.Reset();
            }
        }
        finally
        {
            ReadOnlySequenceBuilderPool<byte>.Return(builder);
        }
    }

    private static ReadOnlySequence<byte> SplitToSegments(int segments, ReadOnlySequenceBuilder<byte> sequenceBuilder,
        ReadOnlyMemory<byte> utf8Json)
    {
        var segmentLength = utf8Json.Length / segments;

        for (int i = segments - 2; i >= 0; i--)
        {
            sequenceBuilder.Add(utf8Json.Slice(0, segmentLength));

            utf8Json = utf8Json.Slice(segmentLength);
        }

        sequenceBuilder.Add(utf8Json);

        return sequenceBuilder.Build();
    }

    private static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        var utf8Reader = new Utf8JsonReader(utf8Json);
        return JsonSerializer.Deserialize<TValue>(ref utf8Reader, options);
    }
}