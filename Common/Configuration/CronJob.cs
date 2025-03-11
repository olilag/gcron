using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Common.Configuration;

/// <summary>
/// Represents days of a week.
/// </summary>
[Flags]
public enum DayOfWeek : byte
{
    None = 0,
    Monday = 1 << 0,
    Tuesday = 1 << 1,
    Wednesday = 1 << 2,
    Thursday = 1 << 3,
    Friday = 1 << 4,
    Saturday = 1 << 5,
    Sunday = 1 << 6,
    All = (1 << 7) - 1
}

public static class DayOfWeekExtensions
{
    public static bool Contains(this DayOfWeek dayOfWeek, System.DayOfWeek weekday)
    {
        if (weekday == System.DayOfWeek.Sunday)
        {
            return (dayOfWeek & DayOfWeek.Sunday) != DayOfWeek.None;
        }
        return (dayOfWeek & (DayOfWeek)(1 << (int)weekday - 1)) != DayOfWeek.None;
    }
}

/// <summary>
/// Represents months of a year.
/// </summary>
[Flags]
public enum Month : ushort
{
    None = 0,
    January = 1 << 0,
    February = 1 << 1,
    March = 1 << 2,
    April = 1 << 3,
    May = 1 << 4,
    June = 1 << 5,
    July = 1 << 6,
    August = 1 << 7,
    September = 1 << 8,
    October = 1 << 9,
    November = 1 << 10,
    December = 1 << 11,
    All = (1 << 12) - 1
}

public static class MonthExtensions
{
    public static int? GetNext(this Month month, int current)
    {
        if (current > 13 || current <= 0)
        {
            return null;
        }

        for (int i = 1; i <= 12; i++)
        {
            var m = (Month)(1 << (i - 1));
            if ((month & m) != Month.None)
            {
                if ((Month)(1 << (current - 1)) == m)
                {
                    return i;
                }
            }
        }
        return null;
    }

    public static int First(this Month month)
    {
        for (int i = 1; i <= 12; i++)
        {
            var m = (Month)(1 << (i - 1));
            if ((month & m) != Month.None)
            {
                return i;
            }
        }
        throw new ArgumentException("No months found");
    }
}

/// <summary>
/// Represents days of a month. Represents it as a bitfield.
/// </summary>
public readonly record struct DayOfMonth : IEnumerable<byte>
{
    private readonly uint _value;

    /// <summary>
    /// Initializes a new instance of <see cref="DayOfMonth" /> struct that represents given days.
    /// </summary>
    /// <param name="days">Days that should be represented. Must be between 1-31.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a value in <see cref="days" /> is outside of 1-31.</exception>
    public DayOfMonth(params byte[] days)
    {
        uint acc = 0;
        foreach (var day in days)
        {
            if (day > 31 || day == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), $"Invalid {nameof(day)} = {day} value. Needs to be between 1-31");
            }
            acc |= 1U << (day - 1);
        }
        _value = acc;
    }

    private DayOfMonth(uint value) => _value = value;

    /// <summary>
    /// Creates a new instance of <see cref="DayOfMonth" /> that represents all days.
    /// </summary>
    /// <returns><see cref="DayOfMonth" /> instance that represents all days.</returns>
    public static DayOfMonth All()
    {
        return new DayOfMonth((uint)((1UL << 31) - 1));
    }

    public IEnumerator<byte> GetEnumerator()
    {
        for (byte day = 1; day <= 31; day++)
        {
            if ((_value & (1U << (day - 1))) != 0)
            {
                yield return day;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int? GetNext(int day)
    {
        if (day > 31 || day < 1)
        {
            return null;
        }

        foreach (var d in this)
        {
            if (d >= day)
            {
                return d;
            }
        }
        return null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{GetType().Name} {{ days: ");
        bool first = true;
        foreach (var day in this)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            sb.Append(day);
            first = false;
        }
        sb.Append(" }");
        return sb.ToString();
    }
}

/// <summary>
/// Represents hours of a day. Represents it as a bitfield.
/// </summary>
public readonly record struct Hour : IEnumerable<byte>
{
    private readonly uint _value;

    /// <summary>
    /// Initializes a new instance of <see cref="Hour" /> struct that represents given hours.
    /// </summary>
    /// <param name="hours">Hours that should be represented. Must be between 0-23.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a value in <see cref="hours" /> is outside of 0-23.</exception>
    public Hour(params byte[] hours)
    {
        uint acc = 0;
        foreach (var hour in hours)
        {
            if (hour > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), $"Invalid {nameof(hour)} = {hour} value. Needs to be between 0-23");
            }
            acc |= 1U << hour;
        }
        _value = acc;
    }

    private Hour(uint value) => _value = value;

    /// <summary>
    /// Creates a new instance of <see cref="Hour" /> that represents all hours.
    /// </summary>
    /// <returns><see cref="Hour" /> instance that represents all hours.</returns>
    public static Hour All()
    {
        return new Hour((1 << 24) - 1);
    }

    public IEnumerator<byte> GetEnumerator()
    {
        for (byte hour = 0; hour <= 23; hour++)
        {
            if ((_value & (1U << hour)) != 0)
            {
                yield return hour;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int? GetNext(int hour)
    {
        if (hour > 23 || hour < 0)
        {
            return null;
        }

        foreach (var d in this)
        {
            if (d >= hour)
            {
                return d;
            }
        }
        return null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{GetType().Name} {{ hours: ");
        bool first = true;
        foreach (var hour in this)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            sb.Append(hour);
            first = false;
        }
        sb.Append(" }");
        return sb.ToString();
    }
}

/// <summary>
/// Represents minutes of an hour. Represents it as a bitfield.
/// </summary>
public readonly record struct Minute : IEnumerable<byte>
{
    private readonly ulong _value;

    /// <summary>
    /// Initializes a new instance of <see cref="Minute" /> struct that represents given minutes.
    /// </summary>
    /// <param name="minutes">Minutes that should be represented. Must be between 0-59.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a value in <see cref="minutes" /> is outside of 0-59.</exception>
    public Minute(params byte[] minutes)
    {
        ulong acc = 0;
        foreach (var minute in minutes)
        {
            if (minute > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), $"Invalid {nameof(minute)} = {minute} value. Needs to be between 0-59");
            }
            acc |= 1UL << minute;
        }
        _value = acc;
    }

    private Minute(ulong value) => _value = value;

    // TODO: rewrite as property
    /// <summary>
    /// Creates a new instance of <see cref="Minute" /> that represents all minutes.
    /// </summary>
    /// <returns><see cref="Minute" /> instance that represents all minutes.</returns>
    public static Minute All()
    {
        return new Minute((1UL << 60) - 1);
    }

    public IEnumerator<byte> GetEnumerator()
    {
        for (byte minute = 0; minute <= 59; minute++)
        {
            if ((_value & (1UL << minute)) != 0)
            {
                yield return minute;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int? GetNext(int minute)
    {
        if (minute > 59 || minute < 0)
        {
            return null;
        }

        foreach (var d in this)
        {
            if (d >= minute)
            {
                return d;
            }
        }
        return null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{GetType().Name} {{ minutes: ");
        bool first = true;
        foreach (var minute in this)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            sb.Append(minute);
            first = false;
        }
        sb.Append(" }");
        return sb.ToString();
    }
}

/// <summary>
/// Represents one job from the configuration.
/// </summary>
public record class CronJob
{
    /// <summary>
    /// Minutes when it should be scheduled.
    /// </summary>
    public required Minute Minutes { get; init; }

    /// <summary>
    /// Hours when it should be scheduled.
    /// </summary>
    public required Hour Hours { get; init; }

    /// <summary>
    /// Days of a month when it should be scheduled.
    /// </summary>
    public required DayOfMonth Days { get; init; }

    /// <summary>
    /// Months when it should be scheduled.
    /// </summary>
    public required Month Months { get; init; }

    /// <summary>
    /// Days of a week when it should be scheduled.
    /// </summary>
    public required DayOfWeek Weekdays { get; init; }

    /// <summary>
    /// Shell command that will be executed.
    /// </summary>
    public required string Command { get; init; }
}
