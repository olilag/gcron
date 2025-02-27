using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Configuration;

namespace Daemon;

// TODO: handle situation where multiple jobs will run at the same time
public class Scheduler
{
    //private HashSet<CronJob> _configuration;
    private PriorityQueue<CronJob, DateTime> _scheduledEvents = new();

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

    public CronJob Peek()
    {
        return _scheduledEvents.Peek();
    }

    public bool TryPeek([MaybeNullWhen(false)] out CronJob element, [MaybeNullWhen(false)] out DateTime priority)
    {
        return _scheduledEvents.TryPeek(out element, out priority);
    }

    public void RescheduleTop()
    {
        if (_scheduledEvents.Count == 0)
        {
            return;
        }
        var top = _scheduledEvents.Peek();
        var priority = GetNextExecution(top, DateTime.Now.AddMinutes(1));
        _ = _scheduledEvents.DequeueEnqueue(top, priority);
    }

    public void LoadConfiguration(HashSet<CronJob> jobs)
    {
        var newEvents = new PriorityQueue<CronJob, DateTime>();
        var now = DateTime.Now.AddMinutes(1);
        foreach (var job in jobs)
        {
            newEvents.Enqueue(job, GetNextExecution(job, now));
        }
        System.Console.WriteLine(newEvents);
        _scheduledEvents = newEvents;
    }
}
