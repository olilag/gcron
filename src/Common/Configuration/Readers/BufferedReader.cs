using System.IO;

namespace Common.Configuration.Readers;

/// <summary>
/// Wraps a <see cref="TextReader"/> and buffers the access to it.
/// </summary>
class BufferedReader : IReader
{
    private readonly TextReader _reader;
    private readonly int _bufferSize;
    private readonly char[] _buffer;
    private int _position;
    private int _validCount;
    public bool EndOfStream { get => _validCount == 0; }

    public BufferedReader(TextReader reader, int bufferSize = 4096)
    {
        _bufferSize = bufferSize;
        _buffer = new char[_bufferSize];
        _reader = reader;
        LoadBuffer();
    }

    private void LoadBuffer()
    {
        _position = 0;
        _validCount = _reader.Read(_buffer);
    }

    private void MovePosition()
    {
        _position++;
        if (_position == _validCount)
        {
            LoadBuffer();
        }
    }

    public char? Read()
    {
        if (_validCount == 0)
        {
            return null;
        }
        var current = _buffer[_position];
        MovePosition();
        return current;
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
