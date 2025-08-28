using System;
using System.Threading;
using System.Threading.Tasks;

public class ConnectivityClientDecorator : IClient
{
    [Serializable]
    public struct Config
    {
        public double FailureRate;
        public double LatencySeconds;
    }

    readonly Random random = new();
    readonly Config config;
    readonly IClient client;

    public bool IsConnected => client.IsConnected;

    public ConnectivityClientDecorator(Config config, IClient client)
    {
        this.config = config;
        this.client = client;
    }

    public async Task<Error> ConnectAsync(CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(config.LatencySeconds), ct);

        if (random.NextDouble() < config.FailureRate)
        {
            return new Error { Message = "connection failed" };
        }

        return await client.ConnectAsync(ct);
    }

    public void Disconnect()
    {
        client.Disconnect();
    }

    public async Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args, CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(config.LatencySeconds), ct);

        if (random.NextDouble() < config.FailureRate)
        {
            return (default, new Error { Message = "message failed" });
        }

        return await client.SendMessage<TArgs, TResult>(message, args, ct);
    }

    public async Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(config.LatencySeconds), ct);

        if (random.NextDouble() < config.FailureRate)
        {
            return new Error { Message = "message failed" };
        }

        return await client.SendMessage(message, args, ct);
    }

}