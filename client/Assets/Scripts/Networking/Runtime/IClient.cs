using System.Threading;
using System.Threading.Tasks;

public interface IClient
{
    Task<Error> ConnectAsync(CancellationToken ct);
    void Disconnect();
    Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args, CancellationToken ct);
    Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct);
    bool IsConnected { get; }
}