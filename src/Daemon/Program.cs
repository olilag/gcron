using System;
using System.IO;
using Common;
using Microsoft.Extensions.Logging;

namespace Daemon;

class Program
{
    static ILoggerFactory CreateLoggerFactory()
    {

        // Create a StreamWriter to write logs to a text file
        return LoggerFactory.Create(builder =>
        {
            // Define the path to the text file
            var logFilePath = Settings.LogFileLocation;

            if (File.Exists(logFilePath))
            {
                var logFileWriter = new StreamWriter(logFilePath, append: true);
                // Add a custom log provider to write logs to text files
                builder.AddProvider(new FileLoggerProvider(logFileWriter));
            }
            else
            {
                Console.Error.WriteLine($"Log file ({logFilePath}) doesn't exists (or insufficient privileges), no logs will be generated");
            }
        });
    }

    static void Main(string[] args)
    {
        var d = new Daemon(CreateLoggerFactory());
        d.MainLoop();
    }
}
