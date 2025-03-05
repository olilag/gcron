using System;
using System.Collections.Generic;
using System.Linq;
using Common.Configuration;

namespace Daemon;

/// <summary>
/// 
/// </summary>
public class Scheduler
{
    private SortedDictionary<DateTime, List<CronJob>> _scheduledEvents = [];

    /// <summary>
    /// 
    /// </summary>
    public bool IsEmpty { get { return _scheduledEvents.Count != 0; } }

    internal static DateTime GetNextExecution(CronJob job, DateTime startTime)
    {
        var startYear = startTime.Year;
        var startMonth = startTime.Month;
        var startDay = startTime.Day;
        var startHour = startTime.Hour;
        var startMinute = startTime.Minute;

        var year = startYear;
        var month = startMonth;
        var day = startDay;
        var hour = startHour;
        var minute = startMinute;

        // Minute
        if (job.Minutes.GetNext(minute) is int mi)
        {
            minute = mi;
        }
        else
        {
            minute = job.Minutes.First();
            hour++;
        }

        // Hour
        if (job.Hours.GetNext(hour) is int h)
        {
            hour = h;
            if (h > startHour)
            {
                minute = job.Minutes.First();
            }
        }
        else
        {
            minute = job.Minutes.First();
            hour = job.Hours.First();
            day++;
        }

        // Day
        var dayRv = job.Days.GetNext(day);
retry_day:
        if (dayRv is int d)
        {
            day = d;
            if (d > startDay)
            {
                minute = job.Minutes.First();
                hour = job.Hours.First();
            }
        }
        else
        {
            minute = job.Minutes.First();
            hour = job.Hours.First();
            day = job.Days.First();
            month++;
        }

        // Month
        if (job.Months.GetNext(month) is int mo)
        {
            month = mo;
            if (mo > startMonth)
            {
                minute = job.Minutes.First();
                hour = job.Hours.First();
                day = job.Days.First();
            }
        }
        else
        {
            minute = job.Minutes.First();
            hour = job.Hours.First();
            day = job.Days.First();
            month = job.Months.First();
            year++;
        }

        var dateChanged = day != startDay || month != startMonth || year != startYear;

        if (day > 28 && dateChanged && day > DateTime.DaysInMonth(year, month))
        {
            dayRv = null;
            goto retry_day;
        }

        var nextExecution = new DateTime(year, month, day, hour, minute, 0);
        // TODO: handle weekdays (do a second search based on weekday and return the one that is closer to today)
        return nextExecution;
    }

    /// <summary>
    /// Returns next events in schedule.
    /// </summary>
    /// <returns>Next events in schedule with their execution time as key.</returns>
    /// <exception cref="InvalidOperationException">When there are no events.</exception>
    public KeyValuePair<DateTime, List<CronJob>> Peek()
    {
        return _scheduledEvents.First();
    }

    private static void AddEvent(SortedDictionary<DateTime, List<CronJob>> dict, CronJob job, DateTime executionTime)
    {
        if (!dict.TryGetValue(executionTime, out var list))
        {
            list = [];
            dict[executionTime] = list;
        }
        list.Add(job);
    }

    /// <summary>
    /// Removes next events from schedule and inserts them back with new execution time.
    /// </summary>
    public void RescheduleTop()
    {
        if (_scheduledEvents.Count == 0)
        {
            return;
        }
        var (oldTime, jobs) = Peek();
        _scheduledEvents.Remove(oldTime);
        var now = DateTime.Now.AddMinutes(1);
        foreach (var job in jobs)
        {
            var priority = GetNextExecution(job, now);
            AddEvent(_scheduledEvents, job, priority);
        }
    }

    /// <summary>
    /// Replaces current jobs in schedule with new ones.
    /// </summary>
    /// <param name="jobs">New jobs to replace old jobs.</param>
    public void LoadConfiguration(HashSet<CronJob> jobs)
    {
        var newEvents = new SortedDictionary<DateTime, List<CronJob>>();
        var now = DateTime.Now.AddMinutes(1);
        foreach (var job in jobs)
        {
            var priority = GetNextExecution(job, now);
            AddEvent(newEvents, job, priority);
        }
        _scheduledEvents = newEvents;
    }
}
