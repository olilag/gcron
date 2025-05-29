using System.IO;
using Microsoft.Extensions.Logging;

namespace Daemon;

class Program
{
    static ILoggerFactory CreateLoggerFactory()
    {
        // Define the path to the text file
        var logFilePath = "gcron.log";

        // Create a StreamWriter to write logs to a text file
        var logFileWriter = new StreamWriter(logFilePath, append: true);
        // Create an ILoggerFactory
        return LoggerFactory.Create(builder =>
        {
            // Add a custom log provider to write logs to text files
            builder.AddProvider(new FileLoggerProvider(logFileWriter));
        });
    }

    static void Main(string[] args)
    {
        var d = new Daemon(CreateLoggerFactory());
        d.MainLoop();
    }
}
