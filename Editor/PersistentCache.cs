using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Editor;

public class PersistentCache : ITomlMetadataProvider
{
    public string? TempFile { get; set; }
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    private readonly static string s_cacheLocation = $"/home/{Environment.UserName}/.cache/gcron/cache.toml";

    public bool Save()
    {
        // create missing directory
        var parentDir = Path.GetDirectoryName(s_cacheLocation);
        if (!string.IsNullOrEmpty(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }
        using var writer = new StreamWriter(s_cacheLocation);
        return Toml.TryFromModel(this, writer, out _);
    }

    public static PersistentCache Load()
    {
        if (!File.Exists(s_cacheLocation))
        {
            return new();
        }
        try
        {
            var f = File.ReadAllText(s_cacheLocation);
            return Toml.ToModel<PersistentCache>(f);
        }
        catch (TomlException)
        {
            return new();
        }
    }
}
