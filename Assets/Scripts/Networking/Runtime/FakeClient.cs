using System;
using System.Threading;
using System.Threading.Tasks;

public interface IFakeServerHandler<TConnState>
{
    (TResult, Error) HandleMessage<TArgs, TResult>(TConnState connState, string message, TArgs args);
    Error HandleMessage<TArgs>(TConnState connState, string message, TArgs args);
}

public class FakeClient : IClient
{
    private readonly IFakeServer fakeServer;
    private int connectionId = -1;

    public bool IsConnected => connectionId != -1;

    public FakeClient(IFakeServer fakeServer)
    {
        this.fakeServer = fakeServer;
    }

    public Task<Error> ConnectAsync(CancellationToken ct)
    {
        connectionId = fakeServer.CreateConnection();
        return Task.FromResult(null as Error);
    }

    public void Disconnect()
    {
        fakeServer.RemoveConnection(connectionId);
        connectionId = -1;
    }

    public Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args, CancellationToken ct)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Client is not connected");
        }

        return Task.FromResult(fakeServer.ReceiveMessage<TArgs, TResult>(connectionId, message, args));
    }

    public Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Client is not connected");
        }

        return Task.FromResult(fakeServer.ReceiveMessage(connectionId, message, args));
    }
}