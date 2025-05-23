using System;
using System.Text;

namespace Common.Configuration.Readers;


enum TokenType
{
    EndOfInput,
    Sequence,
    EndOfLine
}

readonly record struct Token(TokenType Type, string? Value = null)
{
    public Token(string word) : this(TokenType.Sequence, word) { }
}

class TokenReader(IReader reader) : IDisposable
{
    private readonly IReader _reader = reader;
    private readonly StringBuilder _sb = new();
    private bool _prevEndOfLine = false;
    public bool EndOfStream { get; private set; } = false;

    public void Dispose()
    {
        _reader.Dispose();
    }

    public Token ReadToken(bool disableSpace = false)
    {
        if (_prevEndOfLine)
        {
            _prevEndOfLine = false;
            return new(TokenType.EndOfLine);
        }

        while ((_ = _reader.Read()) is char current)
        {
            switch (current)
            {
                // eat separators
                case ' ' when _sb.Length == 0 && !disableSpace:
                    continue;
                // return new sequence
                case ' ' when _sb.Length > 0 && !disableSpace:
                    var seq = _sb.ToString();
                    _sb.Clear();
                    return new(seq);
                // buffer empty, report EOL
                case '\n' when _sb.Length == 0:
                    return new(TokenType.EndOfLine);
                // report EOL on next pass, return seq
                case '\n' when _sb.Length > 0:
                    _prevEndOfLine = true;
                    seq = _sb.ToString();
                    _sb.Clear();
                    return new(seq);
                // add to buffer
                default:
                    _sb.Append(current);
                    continue;
            }
        }

        // report what is left in builder
        if (_sb.Length > 0)
        {
            var seq = _sb.ToString();
            _sb.Clear();
            return new(seq);
        }
        EndOfStream = true;
        // builder was empty
        return new(TokenType.EndOfInput);
    }
}
