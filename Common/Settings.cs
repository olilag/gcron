using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Common;

public class ConfigurationSettings : ITomlMetadataProvider
{
    public string? InitialJobsFile { get; set; }
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
}

public class Config : ITomlMetadataProvider
{
    public ConfigurationSettings Configuration { get; set; } = new();
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

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

public static class Settings
{
    internal readonly static string ConfigFile = $"/home/{Environment.UserName}/.config/gcron/config.toml";
    public readonly static string SpoolLocation = "/var/spool/gcron";
    public readonly static string DefaultEditor = "nano";

    private static Config GetEmptyConfig()
    {
        return new Config
        {
            Configuration = new ConfigurationSettings()
        };
    }

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
