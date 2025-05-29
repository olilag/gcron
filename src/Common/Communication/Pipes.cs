using System;
using System.IO.Pipes;

namespace Common.Communication;

static class PipeConfig
{
    public const string PipeName = "gcrond";
    public const string ServerAddress = ".";
    public const PipeDirection Direction = PipeDirection.InOut;
}

/// <summary>
/// Wraps a <see cref="NamedPipeServerStream"/> to provide <see cref="StringProtocol"/> to exchange data.
/// Uses values from <see cref="PipeConfig"/> to configure the server.
/// </summary>
public sealed class Server : IDisposable
{
    private readonly NamedPipeServerStream _pipeServer;
    private bool _objectDisposed = false;

    public Server()
    {
        _pipeServer = new(PipeConfig.PipeName, PipeConfig.Direction);
    }

    /// <summary>
    /// Waits for a <see cref="Client"/> to connect to this <see cref="Server"/> object.
    /// </summary>
    /// <returns>A <see cref="StringProtocol"/> object to send and receive data between client and serve.</returns>
    /// <inheritdoc cref="NamedPipeServerStream.WaitForConnection"/>
    public StringProtocol WaitForConnection()
    {
        ObjectDisposedException.ThrowIf(_objectDisposed, this);
        _pipeServer.WaitForConnection();
        return new(_pipeServer, this);
    }

    /// <inheritdoc cref="NamedPipeServerStream.Disconnect"/>
    public void Disconnect()
    {
        _pipeServer.Disconnect();
    }

    public void Dispose()
    {
        _objectDisposed = true;
        _pipeServer.Dispose();
    }
}

/// <summary>
/// Wraps a <see cref="NamedPipeClientStream"/> to provide <see cref="StringProtocol"/> to exchange data.
/// Uses values from <see cref="PipeConfig"/> to configure the client.
/// </summary>
public sealed class Client : IDisposable
{
    const int Timeout = 1000;
    private readonly NamedPipeClientStream _pipeClient;
    private bool _objectDisposed = false;

    public Client()
    {
        _pipeClient = new(PipeConfig.ServerAddress, PipeConfig.PipeName, PipeConfig.Direction);
    }

    /// <summary>
    /// Tries to connect to a <see cref="Server"/> object.
    /// </summary>
    /// <returns>A <see cref="StringProtocol"/> object to send and receive data between client and serve.</returns>
    /// <exception cref="TimeoutException">Thrown when couldn't connect to the server.</exception>
    public StringProtocol Connect()
    {
        ObjectDisposedException.ThrowIf(_objectDisposed, this);
        try
        {
            _pipeClient.Connect(Timeout);
        }
        catch (TimeoutException)
        {
            // just rethrow
            throw;
        }
        return new(_pipeClient);
    }

    public void Dispose()
    {
        _objectDisposed = true;
        _pipeClient.Dispose();
    }
}
