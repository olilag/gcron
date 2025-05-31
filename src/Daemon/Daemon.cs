using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Communication;
using Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Daemon;

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

/// <summary>
/// Represents long running program which schedules and executes jobs defined in a schedule.
/// </summary>
public class Daemon(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Daemon>();
    private readonly ILogger _schedulerLogger = loggerFactory.CreateLogger<Scheduler>();
    private readonly ILogger _executorLogger = loggerFactory.CreateLogger<Executor>();
    private readonly Scheduler _scheduler = new();
    private readonly Executor _executor = new(loggerFactory.CreateLogger<Executor>());
    private readonly AutoResetEvent _executeSignal = new(false);
    private CronJob[] _currentJobs = [];
    private bool _configChanged = false;
    private readonly Lock _cfgLock = new();
    private HashSet<CronJob> _configuration = [];
    private readonly Config _appCfg = Settings.Load();

    /// <summary>
    /// Main loop for execute thread.
    /// Waits on a conditional variable (<see cref="AutoResetEvent"/>), then executes all jobs in <see cref="_currentJobs"/>.
    /// Conditional variable is set by scheduling thread.
    /// </summary>
    private void ExecuteThread()
    {
        while (true)
        {
            _executorLogger.LogInformation("Waiting for execute signal");
            _executeSignal.WaitOne();
            var task = Task.Run(() => { });
            foreach (var job in _currentJobs)
            {
                var command = job.Command;
                Task.Run(() => _executor.Execute(command)).Forget();
            }
        }
    }

    /// <summary>
    /// Loads job configuration from last place a correct configuration was saved.
    /// </summary>
    /// <returns>Loaded jobs.</returns>
    private HashSet<CronJob> LoadPersistentConfiguration()
    {
        // store path to last correct config file to a file somewhere
        // load from there here
        var jobFile = _appCfg.Configuration.InitialJobsFile;
        if (!File.Exists(jobFile))
        {
            return [];
        }
        try
        {
            return LoadJobConfiguration(jobFile);
        }
        catch (InvalidConfigurationException)
        {
            return [];
        }
    }

    /// <summary>
    /// Main loop of daemon's scheduling.
    /// Loads initial jobs.
    /// In each iteration it pushes the top job for execution and reschedules it.
    /// Then waits until the next execution time of top job.
    /// If there are no jobs, it sleeps.
    /// <see cref="MainLoop"/> can interrupt this thread if a new configuration is ready.
    /// </summary>
    private void SchedulerThread()
    {
        // load initial jobs
        lock (_cfgLock)
        {
            _configuration = LoadPersistentConfiguration();
            _configChanged = true;
        }
        while (true)
        {
            try
            {
                _cfgLock.Enter();
                if (_configChanged)
                {
                    _schedulerLogger.LogInformation("Loading config");
                    _scheduler.LoadConfiguration(_configuration);
                }
                if (!_scheduler.IsEmpty)
                {
                    // no changes in config => the wake-up wasn't caused by an interrupt from editor => look at top jobs execute and reschedule them
                    if (!_configChanged)
                    {
                        _schedulerLogger.LogInformation("Rescheduling top job");
                        var (_, jobs) = _scheduler.Peek();
                        _currentJobs = new CronJob[jobs.Count];
                        jobs.CopyTo(_currentJobs);
                        _executeSignal.Set();
                        _scheduler.RescheduleTop();
                    }
                    // there were changes => we were interrupted, only calculate time of next execution and sleep until then
                    else
                    {
                        _configChanged = false;
                    }
                    _cfgLock.Exit();
                    var (nextExecution, _) = _scheduler.Peek();
                    var now = DateTime.Now;
                    // add 5 seconds to guarantee that the next wake-up will happen in the correct minute, otherwise scheduling will break
                    nextExecution = nextExecution.AddSeconds(5);
                    var waitFor = nextExecution - now;

                    // wait until next execution
                    _schedulerLogger.LogInformation("Waiting for next execution time: {} - {} = {}", nextExecution, now, waitFor);
                    Thread.Sleep(waitFor);
                    _schedulerLogger.LogInformation("Woken up from waiting for next event");
                }
                else
                {
                    // wait for config changes
                    _cfgLock.Exit();
                    _schedulerLogger.LogInformation("Waiting for change in config");
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (ThreadInterruptedException)
            {
                _schedulerLogger.LogInformation("Caught interrupt exception");
            }
            catch (Exception ex)
            {
                _schedulerLogger.LogError("Caught unexpected exception: {}", ex);
            }
            finally
            {
                // in case there was an an unhandled exception and we are holding the lock 
                if (_cfgLock.IsHeldByCurrentThread)
                    _cfgLock.Exit();
            }
        }
    }

    private static HashSet<CronJob> LoadJobConfiguration(string fileName)
    {
        if (!File.Exists(fileName))
        {
            return [];
        }
        using var parser = new Parser(new StreamReader(fileName));
        return parser.Parse();

    }

    /// <summary>
    /// Daemon's main loop. Waits for changes in config by the editor, then updates the schedule.
    /// Spawns a thread for scheduling.
    /// Spawns a thread for execution.
    /// Its loop consists of waiting for an editor to notify it that a configuration has changed.
    /// Then it checks if the configuration is valid and notifies schedule thread for changes.
    /// </summary>
    public void MainLoop()
    {
        _logger.LogInformation("Starting main loop");
        var executor = new Thread(ExecuteThread);
        var scheduler = new Thread(SchedulerThread);
        executor.Name = "Execute Thread";
        executor.IsBackground = true;
        scheduler.Name = "Scheduler Thread";
        scheduler.IsBackground = true;
        executor.Start();
        scheduler.Start();
        var server = new Server();

        while (true)
        {
            try
            {
                _logger.LogInformation("Waiting for connection");
                using var conn = server.WaitForConnection();
                var configFile = conn.ReadString();
                var newConfiguration = LoadJobConfiguration(configFile);
                // check changes
                if (!newConfiguration.SetEquals(_configuration))
                {
                    lock (_cfgLock)
                    {
                        _configChanged = true;
                        _configuration = newConfiguration;
                        _logger.LogInformation("Waking up scheduler");
                        scheduler.Interrupt();
                        _appCfg.Configuration.InitialJobsFile = configFile;
                        _appCfg.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                // just log the exception and move on
                _logger.LogError("Caught unexpected exception: {}", ex);
            }
        }
    }
}
