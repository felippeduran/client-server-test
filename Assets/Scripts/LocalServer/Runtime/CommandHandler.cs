using System;

public class CommandHandler
{
    readonly IAccountStorage accountStorage;

    [Serializable]
    public class CommandArgs
    {
        ICommand Command;
    }

    [EndpointHandler]
    public Error HandleCommand(ConnectionState connState, ICommand command)
    {
        if (connState.AccountId == null)
        {
            return new Error { Message = "connection not authenticated" };
        }

        var (persistentState, error) = accountStorage.GetPersistentState(connState.AccountId);
        if (error != null)
        {
            return error;
        }

        try
        {
            command.Execute(new PlayerState
            {
                Persistent = persistentState,
                Session = connState.SessionState,
            }, ConfigsProvider.GetHardcodedConfigs());
        }
        catch (MetagameException e)
        {
            return new Error { Message = e.Message };
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