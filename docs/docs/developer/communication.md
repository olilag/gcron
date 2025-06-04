# Inter-process Communication

```text
Communication
├── Pipes.cs - defines a server (for daemon) and a client (for editor) for communication
└── StringProtocol.cs - defines the protocol used to for communication
```

Daemon and Editor are two separate processes.
That means we need to define how the editor will notify the daemon that a change in configuration occurred.

## What is used

For this purpose we use Named Pipes from <xref:System.IO.Pipes> namespace.

Daemon uses a <xref:System.IO.Pipes.NamedPipeServerStream> (used inside <xref:Common.Communication.Server> for easy usage) to establish a server and wait for an editor to connect to it.

Editor uses a <xref:System.IO.Pipes.NamedPipeClientStream> (used inside <xref:Common.Communication.Client> for easy usage) to connect to daemon's server.
Then it can send a message notifying the daemon for a change in configuration.

## Protocol

Named Pipes expose a <xref:System.IO.Stream> which supports reading and writing individual bytes.
Our <xref:Common.Communication.StringProtocol> class uses this basic stream and provides methods for reading and writing strings to the stream.
It uses <xref:System.Text.UnicodeEncoding> for encoding and decoding.
First two bytes sent is the length of the string.
This limits the length of sent string to <xref:System.UInt16.MaxValue?text=ushort.MaxValue>, but that is sufficient for our use case.
