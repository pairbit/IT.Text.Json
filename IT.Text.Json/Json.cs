using IT.Buffers;
using IT.Buffers.Extensions;
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
    public static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> sequence, JsonSerializerOptions? options = null)
    {
        var reader = new Utf8JsonReader(sequence);
        return Deserialize<TValue>(ref reader, options);
    }

    public static TValue? Deserialize<TValue>(in ReadOnlySequence<byte> sequence, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        var reader = new Utf8JsonReader(sequence);
        return Deserialize(ref reader, jsonTypeInfo);
    }

    public static object? Deserialize(in ReadOnlySequence<byte> sequence, Type returnType, JsonSerializerOptions? options = null)
    {
        var reader = new Utf8JsonReader(sequence);
        return Deserialize(ref reader, returnType, options);
    }

    public static object? Deserialize(in ReadOnlySequence<byte> sequence, JsonTypeInfo jsonTypeInfo)
    {
        var reader = new Utf8JsonReader(sequence);
        return Deserialize(ref reader, jsonTypeInfo);
    }

    public static object? Deserialize(in ReadOnlySequence<byte> sequence, Type returnType, JsonSerializerContext context)
    {
        var reader = new Utf8JsonReader(sequence);
        return Deserialize(ref reader, returnType, context);
    }

    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions? options = null)
    {
        TValue? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        TValue? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions? options = null)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));

        object? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, returnType, options);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        object? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerContext context)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));
        if (context is null) throw new ArgumentNullException(nameof(context));

        object? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, returnType, context);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        TValue? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(utf8Json, options);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        TValue? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerOptions? options = null)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));

        object? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, returnType, options);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        object? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerContext context)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));
        if (context is null) throw new ArgumentNullException(nameof(context));

        object? value;

        RentedListShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, returnType, context);
        }
        catch
        {
            RentedListShared.ReturnAndClear();
            throw;
        }

        RentedListShared.Clear();

        return value;
    }

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null,
        int bufferSize = BufferSize.KB_64, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));

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
            await builder.AddAsync(utf8Json, bufferSize, cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize<TValue>(memory.Span, options);
            }
            else
            {
                return Deserialize<TValue>(builder.Build(), options);
            }
        }
        finally
        {
            BufferPool.TryReturn(builder);
        }
    }

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo,
        int bufferSize = BufferSize.KB_64, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (jsonTypeInfo == null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, bufferSize, cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, jsonTypeInfo);
            }
            else
            {
                return Deserialize(builder.Build(), jsonTypeInfo);
            }
        }
        finally
        {
            BufferPool.TryReturn(builder);
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerOptions? options = null,
        int bufferSize = BufferSize.KB_64, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, bufferSize, cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, returnType, options);
            }
            else
            {
                return Deserialize(builder.Build(), returnType, options);
            }
        }
        finally
        {
            BufferPool.TryReturn(builder);
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Stream utf8Json, JsonTypeInfo jsonTypeInfo,
        int bufferSize = BufferSize.KB_64, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (jsonTypeInfo == null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, bufferSize, cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, jsonTypeInfo);
            }
            else
            {
                return Deserialize(builder.Build(), jsonTypeInfo);
            }
        }
        finally
        {
            BufferPool.TryReturn(builder);
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerContext context,
        int bufferSize = BufferSize.KB_64, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, bufferSize, cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, returnType, context);
            }
            else
            {
                return Deserialize(builder.Build(), returnType, context);
            }
        }
        finally
        {
            BufferPool.TryReturn(builder);
        }
    }
}