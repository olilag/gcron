using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Communication;
using Common.Configuration;

namespace Daemon;

/// <summary>
/// Represents long running program which schedules and executes jobs defined in a schedule.
/// </summary>
public class Daemon
{
    private readonly Scheduler _scheduler = new();
    private readonly Executor _executor = new();
    private readonly AutoResetEvent _executeSignal = new(false);
    private CronJob[] _currentJobs = [];
    private bool _configChanged = false;
    private readonly Lock _cfgLock = new();
    private HashSet<CronJob> _configuration = [];
    private readonly Config _appCfg = Settings.Load();

    private void ExecuteThread()
    {
        while (true)
        {
            Console.WriteLine("Waiting for execute signal");
            _executeSignal.WaitOne();
            foreach (var job in _currentJobs)
            {
                var command = job.Command;
                Console.WriteLine($"Executing command: {command}");
                // run commands using Tasks
                // how to handle errors in running tasks?
                // TODO: improve launching one shot tasks
                Task.Run(() => _executor.Execute(command));
            }
        }
    }

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

    private void SchedulerThread()
    {
        // load initial jobs
        lock (_cfgLock)
        {
            _configuration = LoadPersistentConfiguration();
            _scheduler.LoadConfiguration(_configuration);
        }
        while (true)
        {
            try
            {
                _cfgLock.Enter();
                if (_configChanged)
                {
                    Console.WriteLine("Loading config");
                    _scheduler.LoadConfiguration(_configuration);
                }
                if (!_scheduler.IsEmpty)
                {
                    if (!_configChanged)
                    {
                        Console.WriteLine("Rescheduling top job");
                        var (_, jobs) = _scheduler.Peek();
                        _currentJobs = new CronJob[jobs.Count];
                        jobs.CopyTo(_currentJobs);
                        _executeSignal.Set();
                        _scheduler.RescheduleTop();
                    }
                    else
                    {
                        _configChanged = false;
                    }
                    var (nextExecution, _) = _scheduler.Peek();
                    var now = DateTime.Now;
                    var waitFor = nextExecution - now;
                    // wait until next execution
                    Console.WriteLine($"Waiting for next execution time: {nextExecution} - {now} = {waitFor}");
                    // ensure that waitFor is not negative (shouldn't happen, unless crazy rescheduling, I think)
                    _cfgLock.Exit();
                    Thread.Sleep(waitFor);
                    Console.WriteLine("Waked up from waiting for next event");
                }
                else
                {
                    // wait for config changes
                    Console.WriteLine("Waiting for change in config");
                    _cfgLock.Exit();
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Caught interrupt exception");
            }
        }
    }

    private static HashSet<CronJob> LoadJobConfiguration(string fileName)
    {
        using var parser = new Parser(new StreamReader(fileName));
        return parser.Parse();
    }

    /// <summary>
    /// Main loop of daemon. Waits for changes in config by the editor, then updates the schedule. Runs job scheduling in separate thread. Executes jobs in another thread.
    /// </summary>
    public void MainLoop()
    {
        // TODO: maybe better logging
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
                Console.WriteLine("Waiting for connection");
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
                        Console.WriteLine("Wake up scheduler");
                        scheduler.Interrupt();
                        _appCfg.Configuration.InitialJobsFile = configFile;
                        _appCfg.Save();
                    }
                }
                Console.WriteLine("Loop end");
            }
            catch (Exception ex)
            {
                // just log the exception and move on?
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
