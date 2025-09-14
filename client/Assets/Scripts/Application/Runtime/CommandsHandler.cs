using System;
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

            var commandTask = commandQueue.WaitForCommandAsync(cts.Token);
            var refreshTask = Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
            
            await Task.WhenAny(commandTask, refreshTask);
            cts.Cancel();

            if (commandTask.IsCompletedSuccessfully && commandTask.Result != null)
            {
                error = await commandService.Send(commandTask.Result, ct);
                if (error != null)
                {
                    break;
                }
            }
        }

        return error;
    }
}