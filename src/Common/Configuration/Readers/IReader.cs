using System;

namespace Common.Configuration.Readers;

/// <summary>
/// Simple interface for reading individual characters from streams (e.g. a file).
/// </summary>
interface IReader : IDisposable
{
    /// <summary>
    /// Reads a single character from stream.
    /// </summary>
    /// <returns>Character or <see langword="null"/> when end of stream was encountered.</returns>
    public char? Read();
}
