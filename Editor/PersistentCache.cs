using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Editor;

public class PersistentCache : ITomlMetadataProvider
{
    public string? TempFile { get; set; }
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    private const string CacheLocation = "/home/gwen/.cache/gcron/cache.toml";

    public bool Save()
    {
        // create missing directory
        var parentDir = Path.GetDirectoryName(CacheLocation);
        if (!string.IsNullOrEmpty(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }
        using var writer = new StreamWriter(CacheLocation);
        return Toml.TryFromModel(this, writer, out _);
    }

    public static PersistentCache Load()
    {
        if (!File.Exists(CacheLocation))
        {
            return new();
        }
        try
        {
            var f = File.ReadAllText(CacheLocation);
            return Toml.ToModel<PersistentCache>(f);
        }
        catch (TomlException)
        {
            return new();
        }
    }
}
