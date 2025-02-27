using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Common.Communication;
using Common.Configuration;

namespace Daemon;

public class Daemon
{
    private readonly Scheduler _scheduler = new();
    private readonly Executor _executor = new();
    private readonly AutoResetEvent _executeSignal = new(false);
    private CronJob? _currentJob;
    private bool _configChanged = false;
    private HashSet<CronJob> _configuration = [];

    private void ExecuteThread()
    {
        while (true)
        {
            Console.WriteLine("Waiting for execute signal");
            _executeSignal.WaitOne();
            var command = _currentJob!.Command;
            Console.WriteLine($"Executing command: {command}");
            _executor.Execute(command);
        }
    }

    private void SchedulerThread()
    {
        // load initial jobs
        _scheduler.LoadConfiguration(_configuration);
        while (true)
        {
            try
            {
                if (_configChanged)
                {
                    Console.WriteLine("Loading config");
                    _scheduler.LoadConfiguration(_configuration);
                }
                if (_scheduler.Count > 0)
                {
                    if (!_configChanged)
                    {
                        Console.WriteLine("Rescheduling top job");
                        _currentJob = _scheduler.Peek();
                        _scheduler.RescheduleTop();
                        _executeSignal.Set();
                    }
                    else
                    {
                        _configChanged = false;
                    }
                    _ = _scheduler.TryPeek(out _, out var nextExecution);
                    var now = DateTime.Now;
                    var waitFor = nextExecution - now;
                    // wait until next execution
                    Console.WriteLine($"Waiting for next execution time: {nextExecution} - {now} = {waitFor}");
                    Thread.Sleep(waitFor);
                    System.Console.WriteLine("Waked up from waiting for next event");
                }
                else
                {
                    // wait for config changes
                    Console.WriteLine("Waiting for change in config");
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Caught interrupt exception");
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
        executor.Start();
        scheduler.Start();
        var server = new Server();

        while (true)
        {
            Console.WriteLine("Waiting for connection");
            {
                using var conn = server.WaitForConnection();
                var configFile = conn.ReadString();
                using var parser = new Parser(new StreamReader(configFile));
                var newConfiguration = parser.Parse();
                _configChanged = !newConfiguration.SetEquals(_configuration);
                Console.WriteLine(_configChanged);
                Console.WriteLine(newConfiguration);
                _configuration = newConfiguration;
            }
            if (_configChanged)
            {
                Console.WriteLine("Wake up scheduler");
                scheduler.Interrupt();
            }
            server.Disconnect();
            Console.WriteLine("Loop end");
        }
    }
}
