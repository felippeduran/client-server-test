using System.Threading;
using System.Threading.Tasks;

public class CommandSender
{
    readonly IClient client;

    public CommandSender(IClient client)
    {
        this.client = client;
    }

    public async Task<Error> Send(ICommand command, CancellationToken ct)
    {
        return await client.SendMessage($"CommandHandler/HandleCommand", command, ct);
    }
}