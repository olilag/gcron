using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Configuration;

namespace Daemon;

// TODO: handle situation where multiple jobs will run at the same time
public class Scheduler
{
    private SortedDictionary<DateTime, List<CronJob>> _scheduledEvents = [];

    public int Count { get { return _scheduledEvents.Count; } }

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
