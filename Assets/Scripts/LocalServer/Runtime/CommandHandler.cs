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
            if (Math.Abs((timedCommand.Now - DateTime.UtcNow).TotalMilliseconds) > config.MaxTimeDifferenceMilliseconds)
            {
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

    // [Serializable]
    // public class BeginLevelArgs
    // {
    //     public int LevelId;
    // }

    // [Serializable]
    // public class BeginLevelRes { }

    // [EndpointHandler]
    // public (BeginLevelRes, Error) BeginLevel(ConnectionState connState, BeginLevelCommand command)
    // {
    //     if (connState.AccountId == null)
    //     {
    //         return (null, new Error { Message = "connection not authenticated" });
    //     }

    //     var (playerState, error) = accountStorage.GetPlayerState(connState.AccountId);
    //     if (error != null)
    //     {
    //         return (null, error);
    //     }

    //     try
    //     {
    //         command.Execute(playerState, ConfigsProvider.GetHardcodedConfigs());
    //     }
    //     catch (MetagameException e)
    //     {
    //         return (null, new Error { Message = e.Message });
    //     }

    //     return (new BeginLevelRes { }, null);
    // }

    // [Serializable]
    // public class EndLevelArgs
    // {
    //     public bool Success;
    //     public int Score;
    // }

    // [Serializable]
    // public class EndLevelRes { }

    // [EndpointHandler]
    // public (EndLevelRes, Error) EndLevel(ConnectionState connState, EndLevelCommand command)
    // {
    //     if (connState.AccountId == null)
    //     {
    //         return (null, new Error { Message = "connection not authenticated" });
    //     }

    //     var (playerState, error) = accountStorage.GetPlayerState(connState.AccountId);
    //     if (error != null)
    //     {
    //         return (null, error);
    //     }

    //     if (connState.SessionState.CurrentLevelId == null)
    //     {
    //         return (null, new Error { Message = "no level in progress" });
    //     }

    //     try
    //     {
    //         command.Execute(playerState, ConfigsProvider.GetHardcodedConfigs());
    //         connState.SessionState.CurrentLevelId = null;
    //     }
    //     catch (MetagameException e)
    //     {
    //         return (null, new Error { Message = e.Message });
    //     }

    //     return (new EndLevelRes { }, null);
    // }

    // public static IFakeServer CreateServer2<TConnState>(object[] handlers)
    // {
    //     var serverHandler = new FakeServerHandler2<TConnState>(new (string, IHandler<TConnState>)[] {
    //         (, new AuthenticatedMiddleware<>()),
    //     });
    //     return new FakeServer<TConnState>(serverHandler);
    // }
}