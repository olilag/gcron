using System;
using System.IO;
using System.Text;

namespace Common.Communication;

public sealed class StringProtocol(Stream ioStream) : IDisposable
{
    private readonly Stream _ioStream = ioStream;
    private readonly UnicodeEncoding _streamEncoding = new();

    public void Dispose()
    {
        _ioStream.Flush();
    }

    public string ReadString()
    {
        int len;
        len = _ioStream.ReadByte() * 256;
        len += _ioStream.ReadByte();
        var inBuffer = new byte[len];
        _ioStream.ReadExactly(inBuffer, 0, len);

        return _streamEncoding.GetString(inBuffer);
    }

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
