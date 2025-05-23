using System.Diagnostics;

namespace Daemon;

/// <summary>
/// Executes job commands.
/// </summary>
class Executor
{
    /// <summary>
    /// Finds program to run (first substring separated by a space).
    /// </summary>
    /// <param name="command">String from job configuration.</param>
    /// <param name="argsBegin">Position where arguments begin.</param>
    /// <returns>Name of program to run.</returns>
    private static string FindCommand(string command, out int argsBegin)
    {
        for (int i = 0; i < command.Length; i++)
        {
            var current = command[i];
            if (current == ' ')
            {
                argsBegin = i + 1;
                return command[..i];
            }
        }
        argsBegin = command.Length;
        return command;
    }

    // TODO: what to do with exceptions -> nothing, process will be killed
    // TODO: redirect stdout and stderr? -> resolve later
    // TODO: is this process launching correct?
    public void Execute(string command)
    {
        using var proc = new Process();
        var cmd = FindCommand(command, out var argsBegin);
        var args = command[argsBegin..];
        proc.EnableRaisingEvents = false;
        proc.StartInfo.FileName = cmd;
        proc.StartInfo.Arguments = args;
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();
        proc.WaitForExit();
        var o = proc.StandardOutput.ReadToEnd().Trim();
        System.Console.WriteLine($"{cmd}: {o}");
    }
}
