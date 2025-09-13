using System.Threading;
using System.Threading.Tasks;

public class CommandsHandler
{
    readonly ICommandQueue commandQueue;
    readonly ICommandService commandService;

    public CommandsHandler(ICommandQueue commandQueue, ICommandService commandService)
    {
        this.commandQueue = commandQueue;
        this.commandService = commandService;
    }

    public async Task<Error> HandleAsync(CancellationToken ct)
    {
        Error error = null;
        while (!ct.IsCancellationRequested && commandService.CanSend)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var command = await commandQueue.WaitForCommandAsync(cts.Token);
            if (command != null)
            {
                // Debug.Log($"Sending command: {command.GetType().Name}");
                error = await commandService.Send(command, cts.Token);
                if (error != null)
                {
                    break;
                }
            }
        }

        return error;
    }
}