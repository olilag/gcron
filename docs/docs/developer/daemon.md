# Daemon

The daemon is a long running program that keeps track of user's current job configuration, schedules individual jobs for execution and finally executes them.
Our implementation for most of it's runtime waits (sleeps) for a relevant "event" to occur (user changing it's job configuration, the time to execute a job is now,...).
Thus it saves system's resources in contrast to an implementation that would constantly check if a change in configuration occurred or if its time for a job to be executed.

## How it works

Jobs are kept at `/var/spool/gcron/{USERNAME}` where `USERNAME` is name of the user whose configuration that is.

The daemon consists of three threads.
One thread is used to run the <xref:Common.Communication.Server> and listens for editor's notifications for a change in configuration.
This notification contains the name of configuration file that was changed.
It is also responsible to notify the scheduler thread for theses changes.

The scheduler thread keeps track of the time for next execution for each job in user's current configuration.
The jobs along with the time is kept in a <xref:System.Collections.Generic.SortedDictionary`2>.
Next execution time is used as key and so the next jobs to execute are kept on top.
Upon calculating the time of next execution this thread sleeps until that time.
Then it notifies the executor thread with jobs to execute, recalculates theirs execution times and sleeps.

The executor thread is responsible for job execution.
It waits for a signal from scheduler thread.
Then it uses the <xref:System.Threading.Tasks.Task> API ro run each job as a separate task (it also configures the tasks in a way in which they can be "forgotten" - as in no one needs to await/wait for them to finish).

## Scheduling algorithm

Main logic of the algorithm can be found in <xref:Daemon.Scheduler.GetNextExecution(Common.Configuration.CronJob,System.DateTime)>.
This method gets a start time and a job and calculates it's next execution starting from start time.
starts with minutes.

The search goes from least significant parts (minutes) to most significant (months).
For each field (except day of week) the logic is straightforward.
We find the next number in the job definition that's greater or equal to the current date of next execution.
If that number does not exists, we use the first number in that field's definition and increment the next more significant part (from minutes we modify hours and so on) - as this case represent we don't have a valid time in the current hour, day, month or year.

For days we also need to verify if the month contains said day. If it doesn't, we restart the search of the day.

We also calculate a separate execution date using the day of week field.
We take the already calculated minutes, hours and overflown days and add to them the number of days between the current day of week and the next day of week in job's definition.

After calculating both execution dates, we need to decide which of the two to use:

- If day of week is unbounded (every day of week is allowed), we use execution date by days and months.
- If days and months are both unbounded, we use execution by day of week.
- If all are bounded, then we use the one closer to current date.
- If all are unbounded, we use day of week (it shouldn't because both should be same).

## Logging

Daemon logs information about important events to a file located at `/var/log/gcron.log`.

For this it uses the <xref:Microsoft.Extensions.Logging.ILogger> API with a custom file logger implemented by <xref:Daemon.FileLogger>.

Logs are in following format (`<name>` is replaced with a value):

```text
<timestamp> <log level> [<category name>] => <message>
```
