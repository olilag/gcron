using System;
using System.Text;

namespace Common.Configuration.Readers;

/// <summary>
/// Represents type of token.
/// </summary>
enum TokenType
{
    EndOfInput,
    /// <summary>
    /// Represents a sequence of characters.
    /// </summary>
    Sequence,
    EndOfLine
}

/// <summary>
/// We are interested in 3 token types (<see cref="TokenType"/>).
/// They are by default separated by space character (' ').
/// </summary>
/// <param name="Type">Type of token.</param>
/// <param name="Value">String value for <see cref="TokenType.Sequence"/>.</param>
readonly record struct Token(TokenType Type, string? Value = null)
{
    /// <summary>
    /// Constructor for <see cref="TokenType.Sequence"/>.
    /// </summary>
    /// <param name="sequence">Character sequence to save.</param>
    public Token(string sequence) : this(TokenType.Sequence, sequence) { }
}

/// <summary>
/// Wraps a <see cref="IReader"/> and tokenizes the stream.
/// </summary>
/// <param name="reader">Reader to wrap.</param>
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

    /// <summary>
    /// Reads a single token from stream. <see cref="Token"/> for token types.
    /// </summary>
    /// <param name="disableSpace">Set to <see langword="true"/> to not treat space (' ') as token separator.</param>
    /// <returns>Read token.</returns>
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
