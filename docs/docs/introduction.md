# Introduction

Gcron is a .NET implementation of [cron](https://en.wikipedia.org/wiki/Cron) utility.

## Description

The project consists of two parts, an Editor for managing job configuration and a Daemon which keeps track of user's job configuration and executes the jobs.

The following crontab syntax is supported.
Each field can contain comma separated values, ranges (e.g. `1-5`) or `*` (all values allowed).

```text
* * * * * <command to execute>
| | | | |
| | | | day of the week (0–7) (Sunday to Saturday; 7 is also Sunday)
| | | month (1–12)
| | day of the month (1–31)
| hour (0–23)
minute (0–59)
```

It doesn't support multiple users on a single system, each user needs to run it's own daemon instance.

All datetime values are interpreted in current local time.
