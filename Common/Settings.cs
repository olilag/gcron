using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace Common;

public class ConfigurationSettings : ITomlMetadataProvider
{
    public string? JobFile { get; set; }
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
}

public class Config : ITomlMetadataProvider
{
    public ConfigurationSettings Configuration { get; set; } = new();
    // storage for comments and whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
}

public static class ConfigExtensions
{
    public static bool Save(this Config config)
    {
        // handle missing directory
        using var writer = new StreamWriter(Settings.ConfigFile);
        return Toml.TryFromModel(config, writer, out _);
    }
}

public static class Settings
{
    internal const string ConfigFile = "test.toml";
    public static string SpoolLocation { get { return "/var/spool/gcron"; } }

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
