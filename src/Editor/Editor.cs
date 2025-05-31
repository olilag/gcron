using System;
using System.Diagnostics;
using System.IO;
using Common;
using Common.Communication;
using Common.Configuration;

namespace Editor;

static class ErrorCodes
{
    public const int Success = 0;
    public const int NoConfiguration = 1;
    public const int InvalidConfiguration = 2;
    public const int NoConnectionToDaemon = 3;
    public const int IOError = 4;
    public const int NoConfigurationDirectory = 5;
}

/// <summary>
/// Encapsulates commands to edit and manage job configuration.
/// </summary>
public static class Editor
{
    private static string GetCurrentUser()
    {
        return Environment.UserName;
    }

    /// <summary>
    /// Checks if there is a job configuration for a user.
    /// </summary>
    /// <param name="user">User to check for.</param>
    /// <param name="location">Location of the configuration.</param>
    /// <returns><see langword="true"/> if it exists.</returns>
    private static bool JobConfigurationExists(string user, out string location)
    {
        location = Path.Join(Settings.SpoolLocation, user);
        if (!File.Exists(location))
        {
            return false;
        }
        var info = new FileInfo(location);
        // empty file == no configuration
        return info.Length != 0;
    }

    /// <summary>
    /// Lists jobs in current active configuration.
    /// </summary>
    /// <returns>Return code for main.</returns>
    public static int ListJobs()
    {
        var user = GetCurrentUser();
        if (JobConfigurationExists(user, out var jobFile))
        {
            try
            {
                using var parser = new Parser(new StreamReader(jobFile));
                var cfg = parser.Parse();
                foreach (var job in cfg)
                {
                    Console.WriteLine(job.ToString(true));
                }
                return ErrorCodes.Success;
            }
            catch (IOException)
            {
                Console.WriteLine("Error while opening configuration");
                return 4;
            }
        }
        else
        {
            Console.WriteLine($"No configuration for {user}");
            return ErrorCodes.NoConfiguration;
        }
    }

    /// <summary>
    /// Checks if provided file is syntactically correct.
    /// </summary>
    /// <param name="file">File to check.</param>
    /// <returns>Return code for main.</returns>
    public static int CheckSyntax(FileInfo file)
    {
        try
        {
            using var parser = new Parser(new StreamReader(file.Open(FileMode.Open)));
            var cfg = parser.Parse();
            Console.WriteLine("Configuration is ok");
            return ErrorCodes.Success;
        }
        catch (IOException)
        {
            Console.WriteLine("Error while opening configuration");
            return ErrorCodes.IOError;
        }
        catch (InvalidConfigurationException ex)
        {
            Console.WriteLine(ex.Message);
            return ErrorCodes.InvalidConfiguration;
        }
    }

    /// <summary>
    /// Deletes current user's job configuration.
    /// </summary>
    /// <returns>Return code for main.</returns>
    public static int ClearJobs()
    {
        var user = GetCurrentUser();
        if (JobConfigurationExists(user, out var jobFile))
        {
            try
            {
                File.Create(jobFile).Dispose();
                var rv = NotifyDaemon(jobFile);
                if (rv == ErrorCodes.Success)
                {
                    Console.WriteLine("Configuration removed");
                }
                return rv;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return ErrorCodes.IOError;
            }
        }
        else
        {
            Console.WriteLine($"No configuration for {user}");
            return ErrorCodes.NoConfiguration;
        }
    }

    private static string GetEditor()
    {
        var e = Environment.GetEnvironmentVariable("EDITOR") ?? Settings.DefaultEditor;
        return e;
    }

    /// <summary>
    /// Open given file in an editor and returns a <see cref="Process"/>.
    /// </summary>
    /// <param name="fileName">File to edit in editor.</param>
    /// <returns>Process with the editor.</returns>
    private static Process LaunchEditor(string fileName)
    {
        var process = new Process();
        process.StartInfo.FileName = GetEditor();
        process.StartInfo.Arguments = fileName;
        process.StartInfo.UseShellExecute = true;
        process.Start();
        return process;
    }

    /// <summary>
    /// Notifies daemon that the file is changed.
    /// </summary>
    /// <param name="fileName">File that was changed.</param>
    /// <returns>Return code for main.</returns>
    private static int NotifyDaemon(string fileName)
    {
        try
        {
            var client = new Client();
            using var conn = client.Connect();
            conn.WriteString(fileName);
            return ErrorCodes.Success;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Couldn't connect to daemon, make sure daemon is running");
            return ErrorCodes.NoConnectionToDaemon;
        }
    }

    private static int InstallConfig(string sourceFileName, string destinationFileName)
    {
        var dir = Path.GetDirectoryName(destinationFileName);
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Directory for configurations ('{dir}') doesn't exist, create it please (read README.md)");
            return ErrorCodes.NoConfigurationDirectory;
        }
        try
        {
            File.Copy(sourceFileName, destinationFileName, true);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Doesn't have sufficient privileges to write to '{destinationFileName}' where job configuration is, please read README.md to fix this error");
            return ErrorCodes.IOError;
        }
        var rv = NotifyDaemon(destinationFileName);
        if (rv == ErrorCodes.Success)
        {
            Console.WriteLine("Configuration edited successfully");
        }
        return rv;
    }

    /// <summary>
    /// Edits current user's job configuration.
    /// </summary>
    /// <returns>Return code for main.</returns>
    public static int EditJobs()
    {
        using var tmpFile = new TempFile();
        var user = GetCurrentUser();
        if (JobConfigurationExists(user, out var jobFile))
        {
            File.Copy(jobFile, tmpFile, true);
        }
        using var p = LaunchEditor(tmpFile);
        p.WaitForExit();
        try
        {
            using var parser = new Parser(new StreamReader(tmpFile));
            var cfg = parser.Parse();
        }
        catch (InvalidConfigurationException ex)
        {
            Console.WriteLine(ex.Message);
            return ErrorCodes.InvalidConfiguration;
        }
        return InstallConfig(tmpFile, jobFile);
    }
}
