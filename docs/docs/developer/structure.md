# Project Structure

Here are annotated relevant directories in the repository:

```text
gcron
├── docs - docfx documentation
├── src - source code
│   ├── Common - shared parts between editor and daemon
│   │   ├── Communication - inter-process communication between editor and daemon
│   │   └── Configuration - parsing and representation of job configuration
│   ├── Daemon - code for daemon part
│   └── Editor - code for editor part
└── tests
    └── GcronTests - XUnit unit tests
```
