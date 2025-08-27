using System.Threading.Tasks;

public class CommandSender
{
    readonly IClient client;

    public CommandSender(IClient client)
    {
        this.client = client;
    }

    public async Task<Error> Send(ICommand command)
    {
        var commandName = command.GetType().Name;
        commandName = commandName.Replace("Command", string.Empty);
        return await client.SendMessage($"CommandHandler/{commandName}", command);
    }
}