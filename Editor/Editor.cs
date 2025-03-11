using System;
using System.Diagnostics;
using System.IO;
using Common;
using Common.Communication;
using Common.Configuration;

namespace Editor;

public static class Editor
{
    private static string GetCurrentUser()
    {
        return Environment.UserName;
    }

    private static bool ConfigurationExists(string user, out string location)
    {
        location = Path.Join(Settings.SpoolLocation, user);
        return File.Exists(location);
    }

    public static int ListJobs()
    {
        var user = GetCurrentUser();
        if (ConfigurationExists(user, out var jobFile))
        {
            // handle errors
            using var parser = new Parser(new StreamReader(jobFile));
            var cfg = parser.Parse();
            foreach (var job in cfg)
            {
                Console.WriteLine(job);
            }
            return 0;
        }
        else
        {
            Console.WriteLine($"No configuration for {user}");
            return 1;
        }
    }

    public static int CheckSyntax(FileInfo file)
    {
        try
        {
            using var parser = new Parser(new StreamReader(file.Open(FileMode.Open)));
            var cfg = parser.Parse();
            Console.WriteLine("Configuration is ok");
            return 0;
        }
        catch (Exception ex)
        {
            // handle errors
            Console.WriteLine(ex.Message);
            return 2;
        }
    }

    public static int ClearJobs()
    {
        var user = GetCurrentUser();
        if (ConfigurationExists(user, out var jobFile))
        {
            // handle errors
            File.Delete(jobFile);
            return 0;
        }
        else
        {
            Console.WriteLine($"No configuration for {user}");
            return 1;
        }
    }

    private static string GetEditor()
    {
        var e = Environment.GetEnvironmentVariable("EDITOR");
        if (e != null)
        {
            return e;
        }
        // some default
        return "nano";
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

    private static int NotifyDaemon(string fileName)
    {
        try
        {
            var client = new Client();
            using var conn = client.Connect();
            Console.WriteLine("Connected to daemon");
            conn.WriteString(fileName);
            return 0;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Couldn't connect to daemon, make sure daemon is running");
            return 3;
        }
    }

    private static int InstallConfig(string fileName, string destinationFile)
    {
        var dir = Path.GetDirectoryName(destinationFile);
        Directory.CreateDirectory(dir!);
        File.Copy(fileName, destinationFile, true);
        return NotifyDaemon(destinationFile);
    }

    public static int EditJobs()
    {
        // cache this file and reuse later -> solve caching
        var tmpFile = Path.GetTempFileName();
        Console.WriteLine(tmpFile);
        var user = GetCurrentUser();
        if (ConfigurationExists(user, out var jobFile))
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
        catch (Exception ex)
        {
            // handle errors
            Console.WriteLine(ex);
            return 2;
        }
        return InstallConfig(tmpFile, jobFile);
    }
}
