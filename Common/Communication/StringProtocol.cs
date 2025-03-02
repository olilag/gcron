using System;
using System.IO;
using System.Text;

namespace Common.Communication;

/// <summary>
/// Wrapper around a <see cref="Stream"/> used to send and receive data using named pipes. Each message is prepended byt its length in bytes.
/// </summary>
/// <param name="ioStream"><see cref="Stream"/> to wrap.</param>
/// <param name="server"><see cref="Server"/> to automatically disconnect from when disposing.</param>
public sealed class StringProtocol(Stream ioStream, Server? server = null) : IDisposable
{
    private readonly Stream _ioStream = ioStream;
    private readonly Server? _server = server;
    private readonly UnicodeEncoding _streamEncoding = new();

    public void Dispose()
    {
        _ioStream.Flush();
        _server?.Disconnect();
    }

    /// <summary>
    /// Reads a string from the stream.
    /// </summary>
    /// <returns>String read from stream.</returns>
    public string ReadString()
    {
        int len;
        len = _ioStream.ReadByte() * 256;
        len += _ioStream.ReadByte();
        var inBuffer = new byte[len];
        _ioStream.ReadExactly(inBuffer, 0, len);

        return _streamEncoding.GetString(inBuffer);
    }

    /// <summary>
    /// Writes a string to the stream.
    /// </summary>
    /// <param name="outString">String to write to the stream.</param>
    /// <returns>Number of bytes written to the stream.</returns>
    public int WriteString(string outString)
    {
        var outBuffer = _streamEncoding.GetBytes(outString);
        var len = outBuffer.Length;
        if (len > ushort.MaxValue)
        {
            len = ushort.MaxValue;
        }
        _ioStream.WriteByte((byte)(len / 256));
        _ioStream.WriteByte((byte)(len & 255));
        _ioStream.Write(outBuffer, 0, len);
        _ioStream.Flush();

        return outBuffer.Length + 2;
    }
}
