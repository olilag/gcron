using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Editor;

/// <summary>
/// Persistent cache for Editor application.
/// </summary>
class PersistentCache : ITomlMetadataProvider
{
    /// <summary>
    /// Location of recently created temporary file to avoid creating one if it already exists.
    /// </summary>
    public string? TempFile { get; set; }
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    private readonly static string s_cacheLocation = $"/home/{Environment.UserName}/.cache/gcron/cache.toml";

    /// <summary>
    /// Saves current values.
    /// </summary>
    /// <returns>Saved successfully.</returns>
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

    /// <summary>
    /// Loads values from disk.
    /// </summary>
    /// <returns>The cache.</returns>
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
