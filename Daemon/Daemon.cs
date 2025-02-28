using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Communication;
using Common.Configuration;

namespace Daemon;

public class Daemon
{
    private readonly Scheduler _scheduler = new();
    private readonly Executor _executor = new();
    private readonly AutoResetEvent _executeSignal = new(false);
    private readonly AutoResetEvent _configChangedEvent = new(false);
    private CronJob[] _currentJobs = [];
    private HashSet<CronJob> _configuration = [];

    private void ExecuteThread()
    {
        while (true)
        {
            Console.WriteLine("Waiting for execute signal");
            _executeSignal.WaitOne();
            // TODO: fix
            foreach (var job in _currentJobs)
            {
                var command = job.Command;
                Console.WriteLine($"Executing command: {command}");
                // run commands using Tasks
                Task.Run(() => _executor.Execute(command));
            }
        }
    }

    private void SchedulerThread()
    {
        // load initial jobs
        _scheduler.LoadConfiguration(_configuration);
        bool configChanged = false;

        while (true)
        {
            if (configChanged)
            {
                Console.WriteLine("Loading config");
                _scheduler.LoadConfiguration(_configuration);
            }
            if (_scheduler.Count > 0)
            {
                if (!configChanged)
                {
                    Console.WriteLine("Rescheduling top jobs");
                    var (_, jobs) = _scheduler.Peek();
                    _currentJobs = new CronJob[jobs.Count];
                    jobs.CopyTo(_currentJobs);
                    _scheduler.RescheduleTop();
                    _executeSignal.Set();
                }
                else
                {
                    configChanged = false;
                }
                var (nextExecution, _) = _scheduler.Peek();
                var now = DateTime.Now;
                var waitFor = nextExecution - now;
                // wait until next execution
                Console.WriteLine($"Waiting for next execution time: {nextExecution} - {now} = {waitFor}");
                Thread.Sleep(waitFor);
                Console.WriteLine("Waked up from waiting for next event");
            }
            else
            {
                // wait for config changes
                Console.WriteLine("Waiting for change in config");
                _configChangedEvent.WaitOne();
                configChanged = true;
            }
        }
    }

    // TODO: error handling
    public void MainLoop()
    {
        // store last config somewhere
        // wait for notification
        // load configuration
        // schedule
        // sleep
        // on wake-up check time -> if good, execute command reschedule else sleep
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
            Console.WriteLine("Waiting for connection");
            using var conn = server.WaitForConnection();
            var configFile = conn.ReadString();
            using var parser = new Parser(new StreamReader(configFile));
            var newConfiguration = parser.Parse();
            var configChanged = !newConfiguration.SetEquals(_configuration);
            Console.WriteLine(configChanged);
            _configuration = newConfiguration;
            if (configChanged)
            {
                Console.WriteLine("Wake up scheduler");
                _configChangedEvent.Set();
            }
            Console.WriteLine("Loop end");
        }
    }
}
