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

    public async Task ConnectAsync(CancellationToken ct)
    {
        await Task.Delay(1000, ct);
        connectionId = fakeServer.CreateConnection();
    }

    public void Disconnect()
    {
        fakeServer.RemoveConnection(connectionId);
        connectionId = -1;
    }

    public async Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Client is not connected");
        }

        await Task.Delay(1000);
        return fakeServer.ReceiveMessage<TArgs, TResult>(connectionId, message, args);
    }

    public async Task<Error> SendMessage<TArgs>(string message, TArgs args)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Client is not connected");
        }

        await Task.Delay(1000);
        return fakeServer.ReceiveMessage(connectionId, message, args);
    }
}