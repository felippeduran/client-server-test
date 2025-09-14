using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Runtime
{
    public interface ICommand
    {
        public void Execute(PlayerState state, Configs configs);
    }

    public interface ITimedCommand : ICommand
    {
        public DateTime Now { get; }
    }

    public class Message
    {
        public string Route;
        public object Data;
    }

    public interface ICommandExecutor
    {
        void Execute(ICommand command);
    }

    public interface ICommandQueue
    {
        Task<ICommand> WaitForCommandAsync(CancellationToken ct);
    }

    public class ExecutionQueue : ICommandExecutor, ICommandQueue
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
                if (_queue.TryDequeue(out command))
                {
                    break;
                }
                await Task.Yield();
            }
            return command;
        }

        public void Execute(ICommand command)
        {
            command.Execute(playerState, configs);
            _queue.Enqueue(command);
        }
    }
}