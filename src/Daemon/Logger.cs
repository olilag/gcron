using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Daemon;

/// <summary>
/// Provider for <see cref="FileLogger"/>.
/// </summary>
/// <param name="logWriter">Stream for logs.</param>
class FileLoggerProvider(StreamWriter logWriter) : ILoggerProvider
{
    private readonly StreamWriter _logWriter = logWriter;
    private readonly Lock _writerLock = new();

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logWriter, _writerLock);
    }

    public void Dispose()
    {
        _logWriter.Dispose();
    }
}

/// <summary>
/// Logs to a file.
/// </summary>
/// <param name="categoryName">The category name for messages produced by the logger.</param>
/// <param name="logWriter">Stream for logs.</param>
/// <param name="writerLock">Lock for synchronization of writes.</param>
class FileLogger(string categoryName, StreamWriter logWriter, Lock writerLock) : ILogger
{
    private readonly string _categoryName = categoryName;
    private readonly StreamWriter _logWriter = logWriter;
    private readonly Lock _writerLock = writerLock;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Ensure that only information level and higher logs are recorded
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Get the formatted log message
        var message = formatter(state, exception);

        var now = DateTime.Now;

        // Write log messages to text file
        lock (_writerLock)
        {
            _logWriter.WriteLine($"{now:s} {logLevel}: [{_categoryName}] => {message}");
            _logWriter.Flush();
        }
    }
}
