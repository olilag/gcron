using System;
using System.Diagnostics;
using Common;
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
    public void Execute(string command)
    {
        var cmd = Settings.ExecuteShell;
        var args = $"-c \"{command}\"";
        try
        {
            using var proc = new Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            proc.WaitForExit();
            var err = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(err))
            {
                _logger.LogError("Executed: '{}' with args '{}', stderr: '{}'", cmd, args, err.Trim().ReplaceLineEndings(" "));
            }
            else
            {
                _logger.LogInformation("Executed: '{}' with args '{}'", cmd, args);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Execution of '{} {}' failed with exception: {}", cmd, args, ex);
        }
    }
}
