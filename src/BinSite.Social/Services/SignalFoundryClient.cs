using System.Text.Json;
using System.Net.Http.Json;
using BinSite.Social.Models;

namespace BinSite.Social.Services;

/// <summary>Real same-origin Signal API adapter; retains a request identifier after an uncertain failure.</summary>
public sealed class SignalFoundryClient(SocialApi api, HttpClient http, ILogger<SignalFoundryClient> logger)
{
    const string Prefix = "commerce/signal/";
    readonly Dictionary<string, (string Payload, string Id)> Pending = [];
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<List<SignalChallenge>> ListAsync() => (await api.GetAsync<ChallengeList>(Prefix + "challenges")).Challenges;
    public async Task<SignalChallenge> ReadAsync(string id) => (await api.GetAsync<ChallengeEnvelope>(Prefix + "challenges/" + Encode(id))).Challenge;
    public async Task<SignalAttempt> ResumeAsync(string id, string attemptKey = "")
    {
        var attempt = await ScopedAsync<SignalAttempt>(HttpMethod.Get, Prefix + "attempts/" + Encode(id), null, attemptKey);
        // An authoritative read replaces the uncertain local action with its saved evidence.
        Pending.Remove("query:" + id);
        Pending.Remove("finish:" + id);
        return attempt;
    }
    public Task<SignalAttempt> StartAsync(string id) => PostAsync<SignalAttempt>("start:" + id, Prefix + "challenges/" + Encode(id) + "/attempts", requestId => new { requestId });
    public Task<SignalQueryResult> QueryAsync(string attemptId, int[] input, string program, string attemptKey = "")
    {
        SignalFoundryEngine.ValidateBoard(input); program = SignalFoundryEngine.Program(program);
        return PostAsync<SignalQueryResult>("query:" + attemptId, Prefix + "attempts/" + Encode(attemptId) + "/query", requestId => new { requestId, input, program }, attemptKey);
    }
    public Task<SignalAssessment> FinishAsync(string attemptId, int[] answer, string attemptKey = "")
    {
        SignalFoundryEngine.ValidateBoard(answer);
        return PostAsync<SignalAssessment>("finish:" + attemptId, Prefix + "attempts/" + Encode(attemptId) + "/finish", requestId => new { requestId, answer }, attemptKey);
    }
    public async Task<SignalChallenge> PublishAsync(SignalAuthorProject project)
    {
        SignalFoundryEngine.ValidateProject(project);
        var response = await PostAsync<ChallengeEnvelope>("publish", Prefix + "challenges", requestId => new { requestId, mapping = project.Mapping, title = project.Title.Trim(), description = project.Description, examples = project.Examples, target = project.Target, queryLimit = project.QueryLimit, parentId = project.ParentId, authorAlias = project.AuthorAlias.Trim(), allowReuse = project.AllowReuse });
        return response.Challenge;
    }
    public Task<JsonElement> ReportAsync(string id, string reason, string note) => api.PostAsync<JsonElement>(Prefix + "challenges/" + Encode(id) + "/report", new { reason, note });
    public Task<JsonElement> WithdrawAsync(string id) => api.PostAsync<JsonElement>(Prefix + "challenges/" + Encode(id) + "/withdraw", new { });

    async Task<T> PostAsync<T>(string action, string path, Func<string, object> payload, string? attemptKey = null)
    {
        var signature = JsonSerializer.Serialize(payload(""), Json);
        if (!Pending.TryGetValue(action, out var pending) || pending.Payload != signature) pending = (signature, Guid.NewGuid().ToString());
        Pending[action] = pending;
        var response = attemptKey is null ? await api.PostAsync<T>(path, payload(pending.Id)) : await ScopedAsync<T>(HttpMethod.Post, path, payload(pending.Id), attemptKey);
        Pending.Remove(action);
        return response;
    }

    static string Encode(string value) => Uri.EscapeDataString(value);
    async Task<T> ScopedAsync<T>(HttpMethod method, string path, object? body, string attemptKey)
    {
        if (attemptKey.Length != 0 && (attemptKey.Length != 64 || attemptKey.Any(character => !Uri.IsHexDigit(character)))) throw new FormatException("The saved attempt recovery key is invalid. Start a fresh attempt instead.");
        using var request = new HttpRequestMessage(method, "/api/" + path);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (attemptKey.Length > 0) request.Headers.Add("X-Signal-Attempt-Key", attemptKey);
        try
        {
            using var response = await http.SendAsync(request);
            logger.LogInformation("Signal attempt response; method={Method}; status={Status}", method.Method, (int)response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorEnvelope>();
                throw new ApiFailure(response.StatusCode, error?.Error ?? "The attempt action could not be completed.");
            }
            return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("No attempt result was returned.");
        }
        catch (Exception error) { logger.LogWarning("Signal attempt request failed; type={Type}; status={Status}", error.GetType().Name, error is ApiFailure failure ? (int)failure.Status : 0); throw; }
    }
    sealed class ChallengeList { public List<SignalChallenge> Challenges { get; set; } = []; }
    sealed class ChallengeEnvelope { public SignalChallenge Challenge { get; set; } = new(); }
    sealed class ErrorEnvelope { public string Error { get; set; } = ""; }
}
