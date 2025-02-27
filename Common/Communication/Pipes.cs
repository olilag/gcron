using System;
using System.IO.Pipes;

namespace Common.Communication;

static class PipeConfig
{
    public const string PipeName = "gcrond";
    public const string ServerAddress = ".";
    public const PipeDirection Direction = PipeDirection.InOut;
}

public sealed class Server : IDisposable
{
    private readonly NamedPipeServerStream _pipeServer;
    private bool _objectDisposed = false;

    public Server()
    {
        _pipeServer = new(PipeConfig.PipeName, PipeConfig.Direction);
    }

    public StringProtocol WaitForConnection()
    {
        ObjectDisposedException.ThrowIf(_objectDisposed, this);
        _pipeServer.WaitForConnection();
        return new(_pipeServer);
    }

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

public sealed class Client : IDisposable
{
    const int Timeout = 5000;
    private readonly NamedPipeClientStream _pipeClient;
    private bool _objectDisposed = false;

    public Client()
    {
        _pipeClient = new NamedPipeClientStream(PipeConfig.ServerAddress, PipeConfig.PipeName, PipeConfig.Direction);
    }

    public StringProtocol Connect()
    {
        ObjectDisposedException.ThrowIf(_objectDisposed, this);
        try
        {
            _pipeClient.Connect(Timeout);
        }
        catch (TimeoutException)
        {
            // TODO: throw something
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
