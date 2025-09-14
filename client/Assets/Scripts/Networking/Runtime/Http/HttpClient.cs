using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

public class HttpClient : IClient
{
    readonly string baseUrl;
    System.Net.Http.HttpClient httpClient;

    public bool IsConnected => httpClient != null;

    public HttpClient(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public Task<Error> ConnectAsync(CancellationToken ct)
    {
        httpClient = new System.Net.Http.HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5),
            BaseAddress = new Uri(baseUrl),
        };

        return Task.FromResult(null as Error);
    }

    public void Disconnect()
    {
        httpClient?.Dispose();
        httpClient = null;
    }

    public async Task<(TResult, Error)> SendMessage<TArgs, TResult>(string message, TArgs args, CancellationToken ct)
    {
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
        catch (Exception e)
        {
            return (default(TResult), new Error { Message = e.Message });
        }
    }

    public async Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct)
    {
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
        catch (Exception e)
        {
            return new Error { Message = e.Message };
        }
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
}