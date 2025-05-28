# Getting Started

## Dependencies

- .NET 9.0
- Linux support only

## Preparing

Daemon needs access to `/var/spool/gcron` directory. By default `/var/spool` is owned by `root` and so it does not have access to it.
To fix this either create the directory manually and `chown` it to yourself or run this script (will ask for root password).

```bash
#!/bin/bash

umask u=rwx,go=rx
sudo mkdir -p /var/spool/gcron
user_id=$(id -u)
sudo chown "$user_id" /var/spool/gcron
```

## Executing program

1. Launch daemon by running `dotnet run --project src/Daemon`.
2. Launch editor by running `dotnet run --project src/Editor`:
    - `-l` - will list jobs in current configuration
    - `-e` - will open editor (`$EDITOR` or `nano`) to edit current configuration
    - `-r` - will remove current configuration
    - `-T <FILE>` - will check if `<FILE>` contains valid configuration
3. After saving the configuration, daemon will run the jobs according to schedule.
