using System;
using System.Diagnostics;
using System.IO;
using Common;
using Common.Communication;
using Common.Configuration;

namespace Editor;

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
        return File.Exists(location);
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
                    Console.WriteLine(job);
                }
                return 0;
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
            return 1;
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
            return 0;
        }
        catch (IOException)
        {
            Console.WriteLine("Error while opening configuration");
            return 4;
        }
        catch (InvalidConfigurationException ex)
        {
            Console.WriteLine(ex.Message);
            return 2;
        }
    }

    /// <summary>
    /// Deletes current job configuration.
    /// </summary>
    /// <returns>Return code for main.</returns>
    public static int ClearJobs()
    {
        var user = GetCurrentUser();
        if (JobConfigurationExists(user, out var jobFile))
        {
            try
            {
                File.Delete(jobFile);
                return 0;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return 4;
            }
        }
        else
        {
            Console.WriteLine($"No configuration for {user}");
            return 1;
        }
    }

    private static string GetEditor()
    {
        var e = Environment.GetEnvironmentVariable("EDITOR") ?? Settings.DefaultEditor;
        return e;
    }

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
            return 0;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Couldn't connect to daemon, make sure daemon is running");
            return 3;
        }
    }

    private static int InstallConfig(string sourceFileName, string destinationFileName)
    {
        // TODO: handle errors
        var dir = Path.GetDirectoryName(destinationFileName);
        Directory.CreateDirectory(dir!);
        File.Copy(sourceFileName, destinationFileName, true);
        return NotifyDaemon(destinationFileName);
    }

    /// <summary>
    /// Edits current job configuration.
    /// </summary>
    /// <returns>Return code for main.</returns>
    public static int EditJobs()
    {
        using var tmpFile = new TempFile();
        Console.WriteLine(tmpFile);
        var user = GetCurrentUser();
        if (JobConfigurationExists(user, out var jobFile))
        {
            // maybe errors?
            File.Copy(jobFile, tmpFile, true);
        }
        using var p = LaunchEditor(tmpFile);
        // handle errors
        p.WaitForExit();
        try
        {
            using var parser = new Parser(new StreamReader(tmpFile));
            var cfg = parser.Parse();
        }
        catch (InvalidConfigurationException ex)
        {
            // handle errors
            Console.WriteLine(ex.Message);
            return 2;
        }
        return InstallConfig(tmpFile, jobFile);
    }
}
