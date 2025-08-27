using System;
using System.Threading.Tasks;

public class MetagameClient
{
    readonly IClient client;

    public MetagameClient(IClient client)
    {
        this.client = client;
    }

    public async Task<Error> AuthenticateAsync(Account account)
    {
        var (_, error) = await client.SendMessage<AuthenticateArgs, AuthenticateRes>("AuthenticationHandler/Authenticate", new AuthenticateArgs { AccountId = account.Id, AccessToken = account.AccessToken });
        if (error != null)
        {
            return error;
        }

        return null;
    }

    public async Task<(PlayerState, Error)> GetPlayerStateAsync()
    {
        var (res, error) = await client.SendMessage<GetPlayerStateArgs, GetPlayerStateRes>("InitializationHandler/GetPlayerState", new GetPlayerStateArgs { });
        if (error != null)
        {
            return (default, error);
        }

        return (res.PlayerState, null);
    }

    public async Task<(Configs, Error)> GetConfigsAsync()
    {
        var (res, error) = await client.SendMessage<GetConfigsArgs, GetConfigsRes>("InitializationHandler/GetConfigs", new GetConfigsArgs { });
        if (error != null)
        {
            return (default, error);
        }

        return (res.Configs, null);
    }

    public async Task<((PlayerState, Configs), Error)> SynchronizeAsync()
    {
        var stateTask = GetPlayerStateAsync();
        var configsTask = GetConfigsAsync();

        await Task.WhenAll(stateTask, configsTask);

        if (stateTask.Result.Item2 != null)
        {
            return (default, stateTask.Result.Item2);
        }

        if (configsTask.Result.Item2 != null)
        {
            return (default, configsTask.Result.Item2);
        }

        return ((stateTask.Result.Item1, configsTask.Result.Item1), null);
    }
}