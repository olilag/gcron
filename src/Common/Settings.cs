using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Common;

public class ConfigurationSettings : ITomlMetadataProvider
{
    /// <summary>
    /// Load jobs from this file on Daemon startup.
    /// </summary>
    public string? InitialJobsFile { get; set; }
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
}

public class Config : ITomlMetadataProvider
{
    public ConfigurationSettings Configuration { get; set; } = new();
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    /// <summary>
    /// Save this configuration.
    /// </summary>
    /// <returns><see langword="true"/> when saved successfully.</returns>
    public bool Save()
    {
        // create missing directory
        var parentDir = Path.GetDirectoryName(Settings.ConfigFile);
        if (!string.IsNullOrEmpty(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }
        using var writer = new StreamWriter(Settings.ConfigFile);
        return Toml.TryFromModel(this, writer, out _);
    }
}

/// <summary>
/// Handles common settings between Daemon and Editor.
/// </summary>
public static class Settings
{
    /// <summary>
    /// Location of users application configuration.
    /// </summary>
    internal readonly static string ConfigFile = $"/home/{Environment.UserName}/.config/gcron/config.toml";
    /// <summary>
    /// Location where registered job configurations are stored.
    /// </summary>
    public readonly static string SpoolLocation = "/var/spool/gcron";
    /// <summary>
    /// Default editor for cases where it is not defined by env variable EDITOR.
    /// </summary>
    public readonly static string DefaultEditor = "nano";

    private static Config GetEmptyConfig()
    {
        return new Config
        {
            Configuration = new ConfigurationSettings()
        };
    }

    /// <summary>
    /// Load persistent application configuration.
    /// </summary>
    /// <returns>The configuration.</returns>
    public static Config Load()
    {
        if (!File.Exists(ConfigFile))
        {
            return GetEmptyConfig();
        }
        try
        {
            var f = File.ReadAllText(ConfigFile);
            return Toml.ToModel<Config>(f);
        }
        catch (TomlException)
        {
            return GetEmptyConfig();
        }
    }
}
