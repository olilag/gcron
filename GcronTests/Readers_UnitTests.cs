using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Configuration.Readers;

namespace GcronTests;

public class BufferedReader_UnitTests
{
    static string ReadAll(BufferedReader reader)
    {
        var sb = new StringBuilder();
        while (reader.Read() is char c)
        {
            sb.Append(c);
        }
        return sb.ToString();
    }

    [Fact]
    public void BufferedReader_Read()
    {
        // Arrange
        var expected = "abcdef \n ghsas  iwhfsa hjhsak";
        using var reader = new BufferedReader(new StringReader(expected));
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expected, actual);
        Assert.True(reader.EndOfStream);
    }

    [Fact]
    public void BufferedReader_EndOfStream()
    {
        // Arrange
        var expected = "abc";
        using var reader = new BufferedReader(new StringReader(expected));
        // Act + Assert
        reader.Read();
        Assert.False(reader.EndOfStream);
        reader.Read();
        Assert.False(reader.EndOfStream);
        reader.Read();
        Assert.True(reader.EndOfStream);
        reader.Read();
        Assert.True(reader.EndOfStream);
    }

    [Fact]
    public void BufferedReader_Read_EmptyStream()
    {
        // Arrange
        var expected = "";
        using var reader = new BufferedReader(new StringReader(expected));
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expected, actual);
        Assert.True(reader.EndOfStream);
    }

    [Fact]
    public void BufferedReader_Read_SmallBuffer()
    {
        // Arrange
        var expected = "abcdef \n ghsas  iwhfsa hjhsak";
        using var reader = new BufferedReader(new StringReader(expected), 8);
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expected, actual);
        Assert.True(reader.EndOfStream);
    }
}

public class TokenReader_UnitTests
{
    class IReaderWrapper(TextReader reader) : IReader
    {
        public bool EndOfStream { get; private set; } = false;

        public void Dispose()
        {
            reader.Dispose();
        }

        public char? Read()
        {
            var c = reader.Read();
            if (c == -1)
            {
                EndOfStream = true;
                return null;
            }
            return (char)c;
        }
    }

    static List<Token> ReadAll(TokenReader reader)
    {
        var tokens = new List<Token>();
        while (true)
        {
            var token = reader.ReadToken();
            tokens.Add(token);
            if (token.Type == TokenType.EndOfInput)
            {
                break;
            }
        }
        return tokens;
    }

    [Fact]
    public void BufferedReader_Read()
    {
        // Arrange
        var inputString = "abcdef \n ghsas  iwhfsa hjhsak";
        List<Token> expectedTokens = [new("abcdef"), new(TokenType.EndOfLine), new("ghsas"), new("iwhfsa"), new("hjhsak"), new(TokenType.EndOfInput)];
        using var reader = new TokenReader(new IReaderWrapper(new StringReader(inputString)));
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expectedTokens, actual);
        Assert.True(reader.EndOfStream);
    }

    [Fact]
    public void BufferedReader_Read_EmptyStream()
    {
        // Arrange
        var inputString = "";
        List<Token> expectedTokens = [new(TokenType.EndOfInput)];
        using var reader = new TokenReader(new IReaderWrapper(new StringReader(inputString)));
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expectedTokens, actual);
        Assert.True(reader.EndOfStream);
    }

    [Fact]
    public void BufferedReader_Read_MultipleEOL()
    {
        // Arrange
        var inputString = "abcdef \n\n ghsas  iwhfsa hjhsak";
        List<Token> expectedTokens = [new("abcdef"), new(TokenType.EndOfLine), new(TokenType.EndOfLine), new("ghsas"), new("iwhfsa"), new("hjhsak"), new(TokenType.EndOfInput)];
        using var reader = new TokenReader(new IReaderWrapper(new StringReader(inputString)));
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expectedTokens, actual);
        Assert.True(reader.EndOfStream);
    }

    [Fact]
    public void BufferedReader_Read_FlushBeforeEOL()
    {
        // Arrange
        var inputString = "abcdef\n ghsas  iwhfsa hjhsak";
        List<Token> expectedTokens = [new("abcdef"), new(TokenType.EndOfLine), new("ghsas"), new("iwhfsa"), new("hjhsak"), new(TokenType.EndOfInput)];
        using var reader = new TokenReader(new IReaderWrapper(new StringReader(inputString)));
        // Act
        var actual = ReadAll(reader);
        // Assert
        Assert.Equal(expectedTokens, actual);
        Assert.True(reader.EndOfStream);
    }
}
