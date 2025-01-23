using IT.Json.Internal;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace IT.Json;

public static class Json
{
    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions? options = null)
    {
        TValue? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ref Utf8JsonReader reader, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        TValue? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerOptions? options = null)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));

        object? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, returnType, options);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        object? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type returnType, JsonSerializerContext context)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));
        if (context is null) throw new ArgumentNullException(nameof(context));

        object? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(ref reader, returnType, context);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions? options = null)
    {
        TValue? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize<TValue>(utf8Json, options);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static TValue? Deserialize<TValue>(ReadOnlySpan<byte> utf8Json, JsonTypeInfo<TValue> jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        TValue? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerOptions? options = null)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));

        object? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, returnType, options);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo is null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        object? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, jsonTypeInfo);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    public static object? Deserialize(ReadOnlySpan<byte> utf8Json, Type returnType, JsonSerializerContext context)
    {
        if (returnType is null) throw new ArgumentNullException(nameof(returnType));
        if (context is null) throw new ArgumentNullException(nameof(context));

        object? value;

        ArrayPoolShared.Enable();

        try
        {
            value = JsonSerializer.Deserialize(utf8Json, returnType, context);
        }
        catch
        {
            ArrayPoolShared.ReturnAndClear();
            throw;
        }

        ArrayPoolShared.Clear();

        return value;
    }

    /*

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));

        if (utf8Json is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var arraySegment))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var span = arraySegment.AsSpan(checked((int)memoryStream.Position));

            var value = Deserialize<TValue>(span, options);

            memoryStream.Seek(span.Length, SeekOrigin.Current);
            
            return value;
        }
        

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, cancellationToken: cancellationToken).ConfigureAwait(false);

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

    public static async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, JsonTypeInfo<TValue> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (jsonTypeInfo == null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, jsonTypeInfo);
            }
            else
            {
                var seq = builder.Build();
                var reader = new Utf8JsonReader(seq);
                return Deserialize(ref reader, jsonTypeInfo);
            }
        }
        finally
        {
            ReadOnlySequenceBuilder<byte>.Pool.Return(builder);
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, returnType, options);
            }
            else
            {
                var seq = builder.Build();
                var reader = new Utf8JsonReader(seq);
                return Deserialize(ref reader, returnType, options);
            }
        }
        finally
        {
            ReadOnlySequenceBuilder<byte>.Pool.Return(builder);
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Stream utf8Json, JsonTypeInfo jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (jsonTypeInfo == null) throw new ArgumentNullException(nameof(jsonTypeInfo));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, jsonTypeInfo);
            }
            else
            {
                var seq = builder.Build();
                var reader = new Utf8JsonReader(seq);
                return Deserialize(ref reader, jsonTypeInfo);
            }
        }
        finally
        {
            ReadOnlySequenceBuilder<byte>.Pool.Return(builder);
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Stream utf8Json, Type returnType, JsonSerializerContext context, CancellationToken cancellationToken = default)
    {
        if (utf8Json == null) throw new ArgumentNullException(nameof(utf8Json));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var builder = ReadOnlySequenceBuilder<byte>.Pool.Rent();
        try
        {
            await builder.AddAsync(utf8Json, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize(memory.Span, returnType, context);
            }
            else
            {
                var seq = builder.Build();
                var reader = new Utf8JsonReader(seq);
                return Deserialize(ref reader, returnType, context);
            }
        }
        finally
        {
            ReadOnlySequenceBuilder<byte>.Pool.Return(builder);
        }
    }

    */
}