using System.Threading;
using System.Threading.Tasks;

public interface IClient
{
    Task ConnectAsync(CancellationToken ct);
    void Disconnect();
    Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args);
    Task<Error> SendMessage<TArgs>(string message, TArgs args);
    bool IsConnected { get; }
}