using System.Threading;
using System.Threading.Tasks;

public interface ICommandService
{
    bool CanSend { get; }
    Task<Error> Send(ICommand command, CancellationToken ct);
}

public class CommandService : ICommandService
{
    readonly IClient client;

    public bool CanSend => client.IsConnected;

    public CommandService(IClient client)
    {
        this.client = client;
    }

    public async Task<Error> Send(ICommand command, CancellationToken ct)
    {
        return await client.SendMessage($"CommandHandler/HandleCommand", command, ct);
    }
}