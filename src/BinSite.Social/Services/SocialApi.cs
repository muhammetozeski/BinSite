using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace BinSite.Social.Services;

/// <summary>Exposes a server's user-facing error and status without exposing technical exception details.</summary>
public sealed class ApiFailure(HttpStatusCode status, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}

/// <summary>Performs same-origin API operations and logs metadata while excluding all submitted content and secret links.</summary>
public sealed class SocialApi(HttpClient http, ILogger<SocialApi> logger)
{
    const bool IsLogEnabled = true;

    /// <summary>Reads a typed server response; read failures are surfaced for an explicit user retry.</summary>
    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);

    /// <summary>Submits one mutation without automatic retries that could duplicate a successful write.</summary>
    public Task<T> PostAsync<T>(string path, object payload) => SendAsync<T>(HttpMethod.Post, path, payload);

    /// <summary>Validates an API result and returns its typed body; the route identifier and body are excluded from logs.</summary>
    async Task<T> SendAsync<T>(HttpMethod method, string path, object? payload)
    {
        var timer = Stopwatch.StartNew();
        var category = path.Split('/')[0].Split('?')[0];
        using var request = new HttpRequestMessage(method, "/api/" + path);
        if (payload is not null) request.Content = JsonContent.Create(payload);
        try
        {
            if (IsLogEnabled) logger.LogInformation("Social request started; method={Method}; category={Category}; payloadPresent={PayloadPresent}", method, category, payload is not null);
            using var response = await http.SendAsync(request);
            if (IsLogEnabled) logger.LogInformation("Social response received; category={Category}; status={Status}; bytes={Bytes}", category, (int)response.StatusCode, response.Content.Headers.ContentLength);
            if (!response.IsSuccessStatusCode)
            {
                var failure = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
                throw new ApiFailure(response.StatusCode, failure?.Error ?? "İşlem tamamlanamadı.");
            }
            return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("API returned an empty result.");
        }
        catch (Exception error)
        {
            if (IsLogEnabled) logger.LogError("Social request failed; category={Category}; type={ErrorType}; status={Status}", category, error.GetType().Name, error is ApiFailure failure ? (int)failure.Status : 0);
            throw;
        }
        finally
        {
            timer.Stop();
            if (IsLogEnabled) logger.LogInformation("Social request finished; category={Category}; elapsedMs={Elapsed}", category, timer.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>Represents the server's intentionally public error envelope.</summary>
    sealed record ErrorEnvelope(string Error);
}
