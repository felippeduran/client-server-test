using System;
using System.Threading;
using System.Threading.Tasks;

namespace Networking.Runtime.Fake
{
    public class ThrottlingClientDecorator : IClient
    {
        [Serializable]
        public struct Config
        {
            public double FailureRate;
            public double RTTSeconds;
        }

        readonly Random random = new();
        readonly Config config;
        readonly IClient client;

        public bool IsConnected => client.IsConnected;

        public ThrottlingClientDecorator(Config config, IClient client)
        {
            this.config = config;
            this.client = client;
        }

        public async Task<Error> ConnectAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);

            if (random.NextDouble() < config.FailureRate)
            {
                return new Error { Message = "connection failed" };
            }

            var error = await client.ConnectAsync(ct);

            await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);

            return error;
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public async Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args, CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);

            if (random.NextDouble() < config.FailureRate)
            {
                return (default, new Error { Message = "message failed" });
            }

            var (result, error) = await client.SendMessage<TArgs, TResult>(message, args, ct);

            await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);

            return (result, error);
        }

        public async Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);

            if (random.NextDouble() < config.FailureRate)
            {
                return new Error { Message = "message failed" };
            }

            var error = await client.SendMessage(message, args, ct);

            await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);

            return error;
        }

    }
}