using System;

namespace Common.Configuration.Readers;

interface IReader : IDisposable
{
    public bool EndOfStream { get; }
    public char? Read();
}
