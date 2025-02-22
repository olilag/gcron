using System;
using System.Collections.Generic;
using System.Linq;
using Common.Configuration;

namespace Daemon;

public class Scheduler
{
    //private HashSet<CronJob> _configuration;
    private PriorityQueue<CronJob, DateTime> _scheduledEvents = new();

    private static byte GetNextMinute(CronJob job, DateTime startTime, out byte carry)
    {
        foreach (var validMinute in job.Minutes)
        {
            if (validMinute >= startTime.Minute)
            {
                carry = 0;
                return validMinute;
            }
        }
        carry = 1;
        return job.Minutes.First();
    }

    private static byte GetNextHour(CronJob job, DateTime startTime, byte carryIn, out byte carryOut)
    {
        carryOut = 0;
        var currentHour = startTime.Hour + carryIn;
        if (currentHour > 23)
        {
            carryOut = 1;
            currentHour = 0;
        }
        foreach (var validHour in job.Hours)
        {
            if (validHour >= currentHour)
            {
                return validHour;
            }
        }
        carryOut = 1;
        return job.Hours.First();
    }

    private static byte GetNextDayOfMonth(CronJob job, DateTime startTime, byte carryIn, out byte carryOut)
    {
        carryOut = 0;
        var currentDay = startTime.Day + carryIn;
        var maxDay = DateTime.DaysInMonth(startTime.Year, startTime.Month);
        if (currentDay > maxDay)
        {
            carryOut = 1;
            currentDay = 1;
        }
search_begin:
        maxDay = DateTime.DaysInMonth(startTime.Year, startTime.AddMonths(carryOut).Month);
        foreach (var validDay in job.Days)
        {
            if (validDay >= currentDay)
            {
                if (validDay > maxDay)
                {
                    // first valid day thats next is bigger than max allowed day in a month => relaunch search in next month
                    // and increment carry
                    carryOut++;
                    goto search_begin;
                }
                return validDay;
            }
        }
        carryOut++;
        var nextDay = job.Days.First();
        var nextMonth = startTime.Month + 1;
        if (nextMonth > 12)
        {
            nextMonth = 1;
        }
        while (nextDay > DateTime.DaysInMonth(startTime.Year, nextMonth))
        {
            nextMonth = startTime.Month + 1;
            if (nextMonth > 12)
            {
                nextMonth = 1;
            }
            carryOut++;
        }
        return nextDay;
    }

    /*private static System.DayOfWeek GetNextDayOfWeek(CronJob job, DateTime startTime)
    {
        var currentDay = startTime.DayOfWeek;

    }*/

    private static byte GetNextMonth(CronJob job, DateTime startTime, byte carryIn, out byte carryOut)
    {
        carryOut = 0;
        var currentMonth = startTime.Month + carryIn;
        while (currentMonth > 12)
        {
            currentMonth -= 12;
            carryOut++;
        }
        while ((job.Months & (Month)(1 << (currentMonth - 1))) == Month.None)
        {
            currentMonth++;
            if (currentMonth > 12)
            {
                carryOut++;
                currentMonth = 1;
            }
        }
        return (byte)currentMonth;
    }

    private static int GetNextYear(DateTime startTime, byte carryIn)
    {
        return startTime.Year + carryIn;
    }

    internal static DateTime GetNextExecution(CronJob job, DateTime startTime)
    {
        var nextMinute = GetNextMinute(job, startTime, out var carry);
        var nextHour = GetNextHour(job, startTime, carry, out carry);
        var nextDayOfMonth = GetNextDayOfMonth(job, startTime, carry, out carry);
        // var nextDayOfWeek = ; TODO: later
        var nextMonth = GetNextMonth(job, startTime, carry, out carry);
        var nextYear = GetNextYear(startTime, carry);

        var nextExecution = new DateTime(nextYear, nextMonth, nextDayOfMonth, nextHour, nextMinute, 0);
        return nextExecution;
    }

    public CronJob Peek()
    {
        return _scheduledEvents.Peek();
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
        var now = DateTime.Now;
        foreach (var job in jobs)
        {
            newEvents.Enqueue(job, GetNextExecution(job, now));
        }
        _scheduledEvents = newEvents;
    }
}
