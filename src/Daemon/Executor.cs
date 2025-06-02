using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Daemon;

/// <summary>
/// Executes job commands.
/// </summary>
/// <param name="logger">Logger to print logs.</param>
class Executor(ILogger logger)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Executes given command in a shell.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    private void Execute(string command)
    {
        var cmd = Settings.ExecuteShell;
        // escape " in command
        var args = $"-c \"{command.Replace("\"", "\\\"")}\"";
        try
        {
            using var proc = new Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.Start();
            proc.WaitForExit();
            _logger.LogInformation("Executed: '{}' with args {}", cmd, args);
        }
        catch (Exception ex)
        {
            _logger.LogError("Execution of '{} {}' failed with exception: {}", cmd, args, ex);
        }
    }

    public void ExecuteJobs(CronJob[] jobs)
    {
        foreach (var job in jobs)
        {
            var command = job.Command;
            Task.Run(() => Execute(command)).Forget();
        }
    }
}

public static class TaskExtensions
{
    /// <summary>
    /// Observes the task to avoid the UnobservedTaskException event to be raised.
    /// </summary>
    public static void Forget(this Task task)
    {
        if (!task.IsCompleted || task.IsFaulted)
        {
            _ = ForgetAwaited(task);
        }

        async static Task ForgetAwaited(Task task)
        {
            await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }
}
