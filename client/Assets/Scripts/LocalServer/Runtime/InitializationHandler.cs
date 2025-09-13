using System;

public class InitializationHandler
{
    readonly IAccountStorage accountStorage;

    public InitializationHandler(IAccountStorage accountStorage)
    {
        this.accountStorage = accountStorage;
    }

    [EndpointHandler]
    public (GetPlayerStateRes, Error) GetPlayerState(ConnectionState connState, GetPlayerStateArgs args)
    {
        if (connState.AccountId == null)
        {
            return (null, new Error { Message = "connection not authenticated" });
        }

        var (playerState, error) = accountStorage.GetPersistentState(connState.AccountId);
        if (error != null)
        {
            return (null, error);
        }

        return (new GetPlayerStateRes
        {
            PlayerState = new PlayerState
            {
                Persistent = playerState,
                Session = connState.SessionState,
            },
            ServerTime = DateTime.UtcNow,
        }, null);
    }

    [EndpointHandler]
    public (GetConfigsRes, Error) GetConfigs(ConnectionState connState, GetConfigsArgs args)
    {
        if (connState.AccountId == null)
        {
            return (null, new Error { Message = "connection not authenticated" });
        }

        return (new GetConfigsRes { Configs = ConfigsProvider.GetHardcodedConfigs() }, null);
    }

    [Serializable]
    public class BeginLevelArgs
    {
        public int LevelId;
    }

    [Serializable]
    public class BeginLevelRes
    {
        public int LevelId;
    }
}