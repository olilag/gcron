# Editor

This project provides a simple CLI application for job configuration management (equivalent of crontab).
It uses <xref:System.CommandLine> to define and parse options and arguments.

## Configuration management

When editing current configuration, editor copies current configuration to a temporary file and launches user's default file editor (`EDITOR` environment variable or `nano` if missing).
The user will edit the configuration.
Then editor checks if the configuration is valid, moves edited configuration back and notifies daemon for changes.
This notification contains the name of configuration file that was changed.
