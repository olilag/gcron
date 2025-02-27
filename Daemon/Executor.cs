using System.Diagnostics;

namespace Daemon;

public class Executor
{
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

    public void Execute(string command)
    {
        var proc = new Process();
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
        var o = proc.StandardOutput.ReadToEnd();
        System.Console.WriteLine(o);
    }
}
