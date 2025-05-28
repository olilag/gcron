# gcron

Gcron is a .NET implementation of linux [cron](https://en.wikipedia.org/wiki/Cron) utility.

## Description

The project consists of two parts an [Editor](src/Editor) for managing job configuration and [Daemon](src/Daemon) which keeps track of users job configuration and executes them.

The following crontab syntax is supported.

```text
# * * * * * <command to execute>
# | | | | |
# | | | | day of the week (0–7) (Sunday to Saturday; 7 is also Sunday)
# | | | month (1–12)
# | | day of the month (1–31)
# | hour (0–23)
# minute (0–59)
```

## Getting Started

### Dependencies

- .NET 9.0
- Linux support only

### Preparing

Daemon needs access to `/var/spool/gcron` directory. By default `/var/spool` is owned by `root` and so it does not have access to it.
To fix this either create the directory manually and `chown` it to yourself or run this [setup](setup.sh) script (will ask for root password).

### Executing program

1. Launch daemon by running `dotnet run --project src/Daemon`.
2. Launch editor by running `dotnet run --project src/Editor` - it will open an editor to edit your current configuration.
3. After saving the configuration, daemon will run the jobs according to schedule.

## Authors

Oliver Lago

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
