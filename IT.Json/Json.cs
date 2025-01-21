using IT.Buffers;
using IT.Json.Internal;
using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace IT.Json;

public static class Json
{
    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions? options = null)
    {
        TValue? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        TValue? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions? options = null)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));

        object? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, returnType, options);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        object? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerContext context)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));
        if (context is null) throw new ArgumentNullException(nameof(context));

        object? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, returnType, context);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        TValue? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(utf8Json, options);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        TValue? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerOptions? options = null)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));

        object? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, returnType, options);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        object? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerContext context)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));
        if (context is null) throw new ArgumentNullException(nameof(context));

        object? value;

        ArrayPoolByteShared.AddToList();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, returnType, context);
        }
        catch
        {
            ArrayPoolByteShared.ReturnAndClear();
            throw;
        }

        ArrayPoolByteShared.Clear();

        return value;
    }

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        /*
        if (utf8Json is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var span = arraySegment.AsSpan(checked((int)memoryStream.Position));

            var value = Deserialize<TValue>(span, options);

            memoryStream.Seek(span.Length, SeekOrigin.Current);
            
            return value;
        }
        */

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize.KB_64);
            var offset = 0;
            do
            {
                if (offset == buffer.Length)
                {
                    builder.Add(buffer, returnToPool: true);
                    buffer = ArrayPool<byte>.Shared.Rent(BufferSize.GetDoubleCapacity(buffer.Length));
                    offset = 0;
                }

                int read = 0;
                try
                {
                    read = await utf8Json.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // buffer is not added in builder, so return here.
                    ArrayPool<byte>.Shared.Return(buffer);
                    throw;
                }

                offset += read;

                if (read == 0)
                {
                    builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                    break;
                }
            } while (true);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize<TValue>(memory.Span, options);
            }
            else
            {
                var seq = builder.Build();
                var reader = new Utf8JsonReader(seq);
                return Deserialize<TValue>(ref reader, options);
            }
        }
        finally
        {
            ReadOnlySequenceBuilder<byte>.Pool.Return(builder);
        }
    }

    public static async ValueTask<TValue?> Slowly_DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));

        TValue? value;

        var rentedList = new RentedList();

        var jso = options != null
            ? new JsonSerializerOptions(options)
            : new JsonSerializerOptions();
        jso.Converters.Add(rentedList);

        try
        {
            value = await JsonSerializer.DeserializeAsync<TValue>(utf8Json, jso, cancellationToken);
        }
        catch
        {
            rentedList.ReturnAndClear();
            throw;
        }

        rentedList.Clear();

        return value;
    }
}