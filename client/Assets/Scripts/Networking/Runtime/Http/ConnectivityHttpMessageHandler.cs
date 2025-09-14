using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

public class ConnectivityHttpMessageHandler : System.Net.Http.HttpClientHandler
{
    [Serializable]
    public struct ConnectivityConfig
    {
        public double FailureRate;
        public double RTTSeconds;
    }

    readonly Random random = new();
    readonly ConnectivityConfig config;

    public ConnectivityHttpMessageHandler(ConnectivityConfig config)
    {
        this.config = config;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);
        if (random.NextDouble() < config.FailureRate)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
        var response = await base.SendAsync(request, ct);

        await Task.Delay(TimeSpan.FromSeconds(config.RTTSeconds / 2), ct);
        return response;
    }
}