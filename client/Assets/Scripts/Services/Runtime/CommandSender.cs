using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICommandService
{
    bool CanSend { get; }
    Task<Error> Send(ICommand command, CancellationToken ct);
}

[Serializable]
public struct CommandArgs
{
    public string Command;
    public object Data;
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
        var args = new CommandArgs
        {
            Command = command.GetType().Name.Replace("Command", string.Empty),
            Data = command,
        };
        return await client.SendMessage($"CommandHandler/HandleCommand", args, ct);
    }
}