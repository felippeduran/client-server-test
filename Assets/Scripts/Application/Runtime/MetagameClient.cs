using System;
using System.Threading;
using System.Threading.Tasks;

public class MetagameClient
{
    readonly IClient client;

    public MetagameClient(IClient client)
    {
        this.client = client;
    }

    public async Task<Error> AuthenticateAsync(Account account, CancellationToken ct)
    {
        var (_, error) = await client.SendMessage<AuthenticateArgs, AuthenticateRes>("AuthenticationHandler/Authenticate", new AuthenticateArgs { AccountId = account.Id, AccessToken = account.AccessToken }, ct);
        if (error != null)
        {
            return error;
        }

        return null;
    }

    public async Task<(PlayerState, DateTime, Error)> GetPlayerStateAsync(CancellationToken ct)
    {
        var (res, error) = await client.SendMessage<GetPlayerStateArgs, GetPlayerStateRes>("InitializationHandler/GetPlayerState", new GetPlayerStateArgs { }, ct);
        if (error != null)
        {
            return (default, default, error);
        }

        return (res.PlayerState, res.ServerTime, null);
    }

    public async Task<(Configs, Error)> GetConfigsAsync(CancellationToken ct)
    {
        var (res, error) = await client.SendMessage<GetConfigsArgs, GetConfigsRes>("InitializationHandler/GetConfigs", new GetConfigsArgs { }, ct);
        if (error != null)
        {
            return (default, error);
        }

        return (res.Configs, null);
    }

    public async Task<((PlayerState, Configs, DateTime), Error)> SynchronizeAsync(CancellationToken ct)
    {
        var stateTask = GetPlayerStateAsync(ct);
        var configsTask = GetConfigsAsync(ct);

        await Task.WhenAll(stateTask, configsTask);

        if (stateTask.Result.Item3 != null)
        {
            return (default, stateTask.Result.Item3);
        }

        if (configsTask.Result.Item2 != null)
        {
            return (default, configsTask.Result.Item2);
        }

        return ((stateTask.Result.Item1, configsTask.Result.Item1, stateTask.Result.Item2), null);
    }
}