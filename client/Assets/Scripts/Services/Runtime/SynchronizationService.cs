using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public interface ISynchronizationService
{
    delegate void ProgressDelegate(float progress);

    Task<(SynchronizedState, Error)> SynchronizeAsync(ProgressDelegate progressSetter, CancellationToken ct);
}

public struct SynchronizedState
{
    public PlayerState PlayerState;
    public Configs Configs;
    public DateTime ServerTime;
}

[Serializable]
public class GetPlayerStateArgs { }

[Serializable]
public class GetPlayerStateRes
{
    public PlayerState PlayerState;
    public DateTime ServerTime;
}

[Serializable]
public class GetConfigsArgs { }

[Serializable]
public class GetConfigsRes
{
    public Configs Configs;
}

public class SynchronizationService : ISynchronizationService
{
    readonly IClient client;

    public SynchronizationService(IClient client)
    {
        this.client = client;
    }

    public async Task<(SynchronizedState, Error)> SynchronizeAsync(ISynchronizationService.ProgressDelegate progressSetter, CancellationToken ct)
    {
        var stateTask = GetPlayerStateAsync(ct);
        var configsTask = GetConfigsAsync(ct);

        var tasks = new List<Task> { stateTask, configsTask };

        void UpdateProgress(Error error)
        {
            if (error == null)
            {
                progressSetter(tasks.Sum(x => x.IsCompleted ? 1f : 0f) / tasks.Count);
            }
        }

        var _ = stateTask.ContinueWith(t => UpdateProgress(t.Result.Error), TaskContinuationOptions.OnlyOnRanToCompletion);
        var __ = configsTask.ContinueWith(t => UpdateProgress(t.Result.Error), TaskContinuationOptions.OnlyOnRanToCompletion);

        await Task.WhenAll(tasks);

        if (stateTask.Result.Error != null)
        {
            return (default, stateTask.Result.Error);
        }

        if (configsTask.Result.Error != null)
        {
            return (default, configsTask.Result.Error);
        }

        return (new SynchronizedState
        {
            PlayerState = stateTask.Result.PlayerState,
            Configs = configsTask.Result.Configs,
            ServerTime = stateTask.Result.ServerTime,
        }, null);
    }

    async Task<(PlayerState PlayerState, DateTime ServerTime, Error Error)> GetPlayerStateAsync(CancellationToken ct)
    {
        var (res, error) = await client.SendMessage<GetPlayerStateArgs, GetPlayerStateRes>("InitializationHandler/GetPlayerState", new GetPlayerStateArgs { }, ct);
        if (error != null)
        {
            return (default, default, error);
        }

        return (res.PlayerState, res.ServerTime, null);
    }

    async Task<(Configs Configs, Error Error)> GetConfigsAsync(CancellationToken ct)
    {
        var (res, error) = await client.SendMessage<GetConfigsArgs, GetConfigsRes>("InitializationHandler/GetConfigs", new GetConfigsArgs { }, ct);
        if (error != null)
        {
            return (default, error);
        }

        return (res.Configs, null);
    }
}