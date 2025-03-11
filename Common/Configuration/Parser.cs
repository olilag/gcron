using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Configuration.Readers;

namespace Common.Configuration;

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException() { }
    public InvalidConfigurationException(string message) : base(message) { }
    public InvalidConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Class for parsing configuration from a text file.
/// </summary>
/// <param name="reader">Reads from configuration source.</param>
sealed public class Parser(TextReader reader) : IDisposable
{
    private readonly TokenReader _reader = new(new BufferedReader(reader));

    private static List<byte> GetNumbers(string s, IEnumerable<byte> all)
    {
        var numbers = new List<byte>();
        foreach (var m in s.Split(','))
        {
            switch (m.Split('-'))
            {
                case ["*"]:
                    numbers.AddRange(all);
                    break;
                case [var num] when num.All(char.IsAsciiDigit):
                    if (byte.TryParse(num, out var parsed))
                    {
                        numbers.Add(parsed);
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case [var num1, var num2] when num1.All(char.IsAsciiDigit) && num2.All(char.IsAsciiDigit):
                    if (byte.TryParse(num1, out var parsed1) && byte.TryParse(num2, out var parsed2))
                    {
                        numbers.AddRange(Enumerable.Range(parsed1, parsed2 - parsed1 + 1).Select(idx => (byte)idx));
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                default:
                    throw new InvalidDataException("Invalid value");
            }
        }
        return numbers;
    }

    private Minute? ParseMinutes()
    {
        var token = _reader.ReadToken();
        if (token.Type == TokenType.EndOfInput)
        {
            return null;
        }
        if (token.Type != TokenType.Sequence)
        {
            throw new InvalidConfigurationException("Parse error: Invalid format");
        }
        try
        {
            return new([.. GetNumbers(token.Value!, Enumerable.Range(0, 60).Select(idx => (byte)idx))]);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException || ex is InvalidDataException)
            {
                throw new InvalidConfigurationException($"Parse error", ex);
            }
            else
            {
                throw;
            }
        }
    }

    private Hour ParseHours()
    {
        var token = _reader.ReadToken();
        if (token.Type != TokenType.Sequence)
        {
            throw new InvalidConfigurationException("Parse error: Invalid format");
        }
        try
        {
            return new([.. GetNumbers(token.Value!, Enumerable.Range(0, 24).Select(idx => (byte)idx))]);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException || ex is InvalidDataException)
            {
                throw new InvalidConfigurationException($"Parse error", ex);
            }
            else
            {
                throw;
            }
        }
    }

    private DayOfMonth ParseDaysOfMonth()
    {
        var token = _reader.ReadToken();
        if (token.Type != TokenType.Sequence)
        {
            throw new InvalidConfigurationException("Parse error: Invalid format");
        }
        try
        {
            return new([.. GetNumbers(token.Value!, Enumerable.Range(1, 31).Select(idx => (byte)idx))]);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentOutOfRangeException || ex is InvalidDataException)
            {
                throw new InvalidConfigurationException($"Parse error", ex);
            }
            else
            {
                throw;
            }
        }
    }

    private Month ParseMonths()
    {
        var token = _reader.ReadToken();
        if (token.Type != TokenType.Sequence)
        {
            throw new InvalidConfigurationException("Parse error: Invalid format");
        }
        try
        {
            var months = GetNumbers(token.Value!, Enumerable.Range(1, 12).Select(idx => (byte)idx));
            ushort monthReturn = 0;
            foreach (var month in months)
            {
                if (month > 12 || month == 0)
                {
                    throw new ArgumentException($"Invalid {nameof(month)} = {month} value. Needs to be between 1-12");
                }
                monthReturn |= (ushort)(1 << (month - 1));
            }
            return (Month)monthReturn;
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException || ex is InvalidDataException)
            {
                throw new InvalidConfigurationException($"Parse error", ex);
            }
            else
            {
                throw;
            }
        }
    }

    private DayOfWeek ParseDaysOfWeek()
    {
        var token = _reader.ReadToken();
        if (token.Type != TokenType.Sequence)
        {
            throw new InvalidConfigurationException("Parse error: Invalid format");
        }
        try
        {
            var weekdays = GetNumbers(token.Value!, Enumerable.Range(1, 7).Select(idx => (byte)idx));
            ushort weekdaysReturn = 0;
            foreach (var weekday in weekdays)
            {
                if (weekday > 7)
                {
                    throw new ArgumentException($"Invalid {nameof(weekday)} = {weekday} value. Needs to be between 0-7");
                }
                if (weekday == 0)
                {
                    weekdaysReturn |= 1 << 6;
                }
                else
                {
                    weekdaysReturn |= (ushort)(1 << (weekday - 1));
                }
            }
            return (DayOfWeek)weekdaysReturn;
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException || ex is InvalidDataException)
            {
                throw new InvalidConfigurationException($"Parse error", ex);
            }
            else
            {
                throw;
            }
        }
    }

    private string ParseCommand()
    {
        var token = _reader.ReadToken(true);
        if (token.Type != TokenType.Sequence)
        {
            throw new InvalidConfigurationException("Parse error: Invalid format");
        }
        return token.Value!;
    }

    private CronJob? ParseJob()
    {
        if (ParseMinutes() is Minute minute) { }
        else
        {
            return null;
        }
        var hour = ParseHours();
        var dayOfMonth = ParseDaysOfMonth();
        var month = ParseMonths();
        var dayOfWeek = ParseDaysOfWeek();
        var command = ParseCommand();
        return new() { Minutes = minute, Hours = hour, Days = dayOfMonth, Months = month, Weekdays = dayOfWeek, Command = command };
    }

    /// <summary>
    /// Parses the configuration. Should be called only once.
    /// </summary>
    /// <returns>Set of parsed jobs.</returns>
    /// <exception cref="InvalidConfigurationException">Configuration has invalid format.</exception>
    /// <exception cref="IOException">Error while reading configuration file.</exception>
    /// <seealso cref="CronJob" />
    public HashSet<CronJob> Parse()
    {
        HashSet<CronJob> jobs = [];
        while (!_reader.EndOfStream)
        {
            var job = ParseJob();
            if (job is null)
            {
                break;
            }
            jobs.Add(job);
            var token = _reader.ReadToken();
            if (token.Type != TokenType.EndOfLine && token.Type != TokenType.EndOfInput)
            {
                throw new InvalidConfigurationException("Parse error: Invalid format");
            }
        }
        return jobs;
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
