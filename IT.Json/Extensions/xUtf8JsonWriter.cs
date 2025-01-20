using System;
#if NET8_0_OR_GREATER
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
#endif
using System.Text.Json;

namespace IT.Json.Extensions;

public static class xUtf8JsonWriter
{
    /// <exception cref="ArgumentException"/>
    /// <exception cref="InvalidOperationException"/>
    public static void WriteBase64(this Utf8JsonWriter writer, ReadOnlySpan<byte> bytes)
    {
#if NET8_0_OR_GREATER
        writer.WriteBase64Chunked(bytes);
#else
        writer.WriteBase64StringValue(bytes);
#endif
    }

#if NET8_0_OR_GREATER

    private const byte Quote = (byte)'"';

    public static void WriteBase64Chunked(this Utf8JsonWriter writer, ReadOnlySpan<byte> span)
    {
        if (writer.Options.Indented)
        {
            writer.WriteStartIndented(Quote);
        }
        else
        {
            writer.WriteStartMinimized(Quote);
        }

        var bufferWriter = new Utf8JsonBufferWriter(writer);

        OperationStatus status;
        do
        {
            var utf8 = bufferWriter.GetSpan(4);

            status = Base64.EncodeToUtf8(span, utf8, out var bytesConsumed, out var bytesWritten, false);

            if (status == OperationStatus.InvalidData) throw new InvalidOperationException();

            bufferWriter.Advance(bytesWritten);

            span = span[bytesConsumed..];

        } while (status == OperationStatus.DestinationTooSmall);

        if (status == OperationStatus.NeedMoreData)
        {
            Debug.Assert(span.Length == 1 || span.Length == 2);

            var utf8 = bufferWriter.GetSpan(4);

            if (Base64.EncodeToUtf8(span, utf8, out var bytesConsumed, out var bytesWritten) != OperationStatus.Done)
                throw new InvalidOperationException();

            bufferWriter.Advance(bytesWritten);

            Debug.Assert(bytesConsumed == span.Length);
            Debug.Assert(bytesWritten == 4);
        }

        writer.WriteEndMinimized(Quote);
        writer.SetFlagToAddListSeparatorBeforeNextItem();
        writer.TokenType() = JsonTokenType.String;
    }

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L52
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_memory")]
    internal extern static ref Memory<byte> GetMemory(this Utf8JsonWriter writer);

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L56
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_tokenType")]
    private extern static ref JsonTokenType TokenType(this Utf8JsonWriter writer);

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L87
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_currentDepth")]
    private extern static ref int CurrentDepth(this Utf8JsonWriter writer);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_BytesPending")]
    internal extern static void SetBytesPending(this Utf8JsonWriter writer, int value);

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L619
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "WriteStartMinimized")]
    private extern static void WriteStartMinimized(this Utf8JsonWriter writer, byte token);

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L682
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "WriteStartIndented")]
    private extern static void WriteStartIndented(this Utf8JsonWriter writer, byte token);

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L1053
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "WriteEndMinimized")]
    private extern static void WriteEndMinimized(this Utf8JsonWriter writer, byte token);

    //https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Writer/Utf8JsonWriter.cs#L1186
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Grow")]
    internal extern static void Grow(this Utf8JsonWriter writer, int requiredSize);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SetFlagToAddListSeparatorBeforeNextItem")]
    internal extern static void SetFlagToAddListSeparatorBeforeNextItem(this Utf8JsonWriter writer);

#endif
}