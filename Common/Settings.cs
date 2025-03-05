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
    public ConfigurationSettings? Configuration { get; set; }
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
            var c = Toml.ToModel<Config>(f);
            c.Configuration ??= new();
            return c;
        }
        catch (TomlException)
        {
            return GetEmptyConfig();
        }
    }
}
