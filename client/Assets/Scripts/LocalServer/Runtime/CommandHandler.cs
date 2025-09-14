using System;

public class CommandHandler
{
    [Serializable]
    public struct Config
    {
        public double MaxTimeDifferenceMilliseconds;
    }

    readonly Config config;
    readonly IAccountStorage accountStorage;

    public CommandHandler(Config config, IAccountStorage accountStorage)
    {
        this.config = config;
        this.accountStorage = accountStorage;
    }

    [EndpointHandler]
    public Error HandleCommand(ConnectionState connState, CommandArgs args)
    {
        if (connState.AccountId == null)
        {
            return new Error { Message = "connection not authenticated" };
        }

        var command = (ICommand)args.Data;
        if (command is ITimedCommand timedCommand)
        {
            var timeDifference = Math.Abs((timedCommand.Now - DateTime.UtcNow).TotalMilliseconds);
            if (timeDifference > config.MaxTimeDifferenceMilliseconds)
            {
                Logger.Log("Synchronized. Time difference: " + timeDifference + "ms");
                return new Error { Message = "command timestamp is too far" };
            }
        }

        var (persistentState, error) = accountStorage.GetPersistentState(connState.AccountId);
        if (error != null)
        {
            return error;
        }

        var playerState = new PlayerState
        {
            Persistent = persistentState,
            Session = connState.SessionState,
        };

        try
        {
            Logger.Log($"Executing command server side: {command.GetType().Name}");
            command.Execute(playerState, ConfigsProvider.GetHardcodedConfigs());
        }
        catch (MetagameException e)
        {
            return new Error { Message = e.Message };
        }

        connState.SessionState = playerState.Session;
        error = accountStorage.SetPersistentState(connState.AccountId, playerState.Persistent);
        if (error != null)
        {
            return error;
        }

        return null;
    }
}