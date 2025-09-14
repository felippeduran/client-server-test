using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Utilities.Runtime.Logging;

namespace Networking.Runtime.Http
{
    public class HttpClient : IClient
    {
        readonly Config config;
        System.Net.Http.HttpClient httpClient;
        CancellationTokenSource workerCts;

        public bool IsConnected => httpClient != null;

        public HttpClient(Config config)
        {
            this.config = config;
        }

        public Task<Error> ConnectAsync(CancellationToken ct)
        {
            Logger.Log("Connecting...");
            httpClient = CreateHttpClient(config);

            workerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _ = HeartbeatWorkerAsync(workerCts.Token);

            return Task.FromResult(null as Error);
        }

        public void Disconnect()
        {
            Logger.Log("Disconnecting...");
            workerCts?.Cancel();
            workerCts?.Dispose();
            workerCts = null;
            httpClient?.Dispose();
            httpClient = null;
        }

        public async Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args, CancellationToken ct)
        {
            if (!IsConnected)
            {
                return (default(TResult), new Error { Message = "client is not connected" });
            }

            var body = JsonSerializer.Serialize(args, GetJsonSerializerOptions());
            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(message, content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return (default(TResult), new Error { Message = "failed to send message" });
                }

                if (response.Headers.TryGetValues("X-Session-Id", out var sessionIdValues))
                {
                    httpClient.DefaultRequestHeaders.Add("X-Session-Id", sessionIdValues);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TResult>(responseContent, GetJsonSerializerOptions());
                return (result, null);
            }
            catch (TaskCanceledException)
            {
                return (default(TResult), new Error { Message = "request canceled" });
            }
            catch (Exception e)
            {
                return (default(TResult), new Error { Message = e.Message });
            }
        }

        public async Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct)
        {
            if (!IsConnected)
            {
                return new Error { Message = "client is not connected" };
            }

            var body = JsonSerializer.Serialize(args, GetJsonSerializerOptions());
            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(message, content, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return new Error { Message = "failed to send message" };
                }

                if (response.Headers.TryGetValues("X-Session-Id", out var sessionIdValues))
                {
                    httpClient.DefaultRequestHeaders.Add("X-Session-Id", sessionIdValues);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return new Error { Message = "request canceled" };
            }
            catch (Exception e)
            {
                return new Error { Message = e.Message };
            }
        }

        async Task HeartbeatWorkerAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (!httpClient.DefaultRequestHeaders.TryGetValues("X-Session-Id", out var sessionIdValues))
                    {
                        await Task.Yield();
                        continue;
                    }

                    var error = await HeartbeatAsync(ct);
                    if (error != null)
                    {
                        Logger.LogWarning("Failed to send heartbeat. Retrying...");

                        error = await HeartbeatAsync(ct);
                        if (error != null)
                        {
                            Logger.LogError("Failed to send heartbeat: " + error.Message);
                            Disconnect();
                            break;
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }
        }

        async Task<Error> HeartbeatAsync(CancellationToken ct)
        {
            if (!IsConnected)
            {
                return new Error { Message = "client is not connected" };
            }

            try
            {
                var response = await httpClient.PostAsync("HeartbeatHandler/Heartbeat", new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"), ct);
                if (!response.IsSuccessStatusCode)
                {
                    return new Error { Message = "failed to send heartbeat" };
                }
            }
            catch (TaskCanceledException)
            {
                return new Error { Message = "request canceled" };
            }
            catch (Exception e)
            {
                return new Error { Message = e.Message };
            }

            return null;
        }

        static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
            };
        }

        static HttpMessageHandler CreateHandler(Config config)
        {
            HttpMessageHandler handler = null;
            if (config.UseConnectivity)
            {
                handler = new ConnectivityHttpMessageHandler(config.Connectivity) { UseCookies = false };
            }
            else
            {
                handler = new HttpClientHandler { UseCookies = false };
            }
            return handler;
        }

        static System.Net.Http.HttpClient CreateHttpClient(Config config)
        {
            return new System.Net.Http.HttpClient(CreateHandler(config), true)
            {
                Timeout = config.Timeout,
                BaseAddress = new Uri(config.BaseUrl),
            };
        }
    }
}