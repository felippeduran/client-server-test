using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICommand
{
    public void Execute(PlayerState state, Configs configs);
}

// public interface ICommand<TRes> : ICommand where TRes : new()
// {
//     public new TRes Execute(PlayerState state, LevelConfig[] configs);
// }

public class Message
{
    public string Route;
    public object Data;
}

public class ExecutionQueue
{
    readonly PlayerState playerState;
    readonly Configs configs;
    readonly Queue<ICommand> _queue = new();

    public ExecutionQueue(PlayerState playerState, Configs configs)
    {
        this.playerState = playerState;
        this.configs = configs;
    }

    public async Task<ICommand> WaitForCommandAsync(CancellationToken ct)
    {
        ICommand command = null;
        while (!ct.IsCancellationRequested)
        {
            if (!_queue.TryDequeue(out command))
            {
                await Task.Yield();
            }
        }
        return command;
    }

    public void Execute(ICommand command)
    {
        command.Execute(playerState, configs);
        _queue.Enqueue(command);
    }

    // public TRes Execute<TRes>(ICommand<TRes> command) where TRes : new()
    // {
    //     TRes res = command.Execute(playerState, levelConfigs);
    //     _queue.Enqueue(command);
    //     return res;
    // }
}