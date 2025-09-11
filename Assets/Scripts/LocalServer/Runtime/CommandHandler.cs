using System;
using UnityEngine;

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
    public Error HandleCommand(ConnectionState connState, ICommand command)
    {
        if (connState.AccountId == null)
        {
            return new Error { Message = "connection not authenticated" };
        }

        if (command is ITimedCommand timedCommand)
        {
            var timeDifference = Math.Abs((timedCommand.Now - DateTime.UtcNow).TotalMilliseconds);
            if (timeDifference > config.MaxTimeDifferenceMilliseconds)
            {
                Debug.Log("Synchronized. Time difference: " + timeDifference + "ms");
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
            Debug.Log($"Executing command server side: {command.GetType().Name}");
            Debug.Log($"Server session state before: {playerState.Session.CurrentLevelId}");
            Debug.Log($"Server Persistent state before: {playerState.Persistent.LevelProgression.Statistics.Count}");
            command.Execute(playerState, ConfigsProvider.GetHardcodedConfigs());
            Debug.Log($"Server session state after: {playerState.Session.CurrentLevelId}");
            Debug.Log($"Server Persistent state after: {playerState.Persistent.LevelProgression.Statistics.Count}");
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