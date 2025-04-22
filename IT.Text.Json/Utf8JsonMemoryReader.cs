using IT.Text.Json.Extensions;
using System;
using System.Text.Json;

namespace IT.Text.Json;

public ref struct Utf8JsonMemoryReader
{
    private Utf8JsonReader _reader;
    private readonly Memory<byte> _memory;

    public readonly Utf8JsonReader Reader => _reader;

    public readonly Memory<byte> Buffer => _memory;

    public readonly Memory<byte> ValueMemory => _memory.Slice(ValueStart, _reader.ValueSpan.Length);

    public readonly int ValueStart => checked((int)_reader.TokenStartIndex + _reader.TokenType.GetOffset());

    public readonly ReadOnlySpan<byte> ValueSpan => _reader.ValueSpan;

    public readonly int BytesConsumed => checked((int)_reader.BytesConsumed);

    public readonly int TokenStartIndex => checked((int)_reader.TokenStartIndex);

    public readonly int CurrentDepth => _reader.CurrentDepth;

    public readonly JsonTokenType TokenType => _reader.TokenType;

    public readonly bool ValueIsEscaped => _reader.ValueIsEscaped;

    public readonly bool IsFinalBlock => _reader.IsFinalBlock;

    public readonly JsonReaderState CurrentState => _reader.CurrentState;

    public Utf8JsonMemoryReader(Memory<byte> memory, JsonReaderOptions options = default)
    {
        _reader = new Utf8JsonReader(memory.Span, options);
        _memory = memory;
    }

    public Utf8JsonMemoryReader(Memory<byte> memory, bool isFinalBlock, JsonReaderState state)
    {
        _reader = new Utf8JsonReader(memory.Span, isFinalBlock, state);
        _memory = memory;
    }

    public bool Read() => _reader.Read();

    public void Skip() => _reader.Skip();

    public bool TrySkip() => _reader.TrySkip();
}