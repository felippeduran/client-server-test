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
        httpClient = new System.Net.Http.HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);

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

    public async Task<Error> SendMessage<TArgs>(string message, TArgs args, CancellationToken ct)
    {
        var body = JsonSerializer.Serialize(args, GetJsonSerializerOptions());
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

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