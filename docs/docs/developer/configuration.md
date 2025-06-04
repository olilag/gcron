# Job Configuration and Parsing

```text
Configuration
├── CronJob.cs - defines a job representation in code
├── Parser.cs - logic for parsing a job configuration
└── Readers - internal helper classes for parsing
    ├── BufferedReader.cs
    ├── IReader.cs
    └── TokenReader.cs
```

## Job configuration format

The job configuration consists of multiple lines, each containing a single job.
A single job consists of multiple fields:

```text
* * * * * <command to execute>
| | | | |
| | | | day of the week (0–7) (Sunday to Saturday; 7 is also Sunday)
| | | month (1–12)
| | day of the month (1–31)
| hour (0–23)
minute (0–59)
```

Each field can contain comma separated values, ranges (e.g. `1-5`) or `*` (all values allowed).
Command to execute is an arbitrary string to be interpreted as a shell command.
Fields are separated by at least a single space.

## Job configuration parsing

The `Parser` first uses `TokenReader` to read individual tokens (`EndOfInput`, `EndOfLine`, `Sequence` - a sequence of characters).
The `Sequence` token is used to parse individual fields (according to each field's constraints).
The `EndOfLine` token is used to separate individual jobs.
In case there is an error in the configuration an exception is thrown.
