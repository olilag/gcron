using System;
using System.IO;
using System.Text;

namespace Editor;

/// <summary>
/// Represents a temporary file for crontab. Implements IDisposable to delete the file on Dispose.
/// </summary>
class TempFile : IDisposable
{
    private readonly string _location;
    private readonly Random _rng = new();
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int RandomSize = 8;

    private string RandomId()
    {
        var sb = new StringBuilder(RandomSize);
        for (int i = 0; i < RandomSize; i++)
        {
            var idx = _rng.Next(Alphabet.Length);

            sb.Append(Alphabet[idx]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Creates a new file in systems temporary directory.
    /// </summary>
    public TempFile()
    {
        _location = Path.GetTempPath() + "crontab." + RandomId();
        while (File.Exists(_location))
        {
            _location = Path.GetTempPath() + "crontab." + RandomId();
        }
        File.Create(_location).Dispose();
    }

    public override string ToString()
    {
        return _location;
    }

    public static implicit operator string(TempFile f) => f._location;

    public void Dispose()
    {
        if (File.Exists(_location))
            File.Delete(_location);
    }
}
