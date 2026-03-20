using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GestAI.Web.Dtos;

namespace GestAI.Web;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public bool IsBusy { get; private set; }
    public event Action<bool>? OnBusyChanged;
    private void SetBusy(bool v) { IsBusy = v; OnBusyChanged?.Invoke(v); }

    public event Action<string>? OnToast;
    public void Toast(string message) => OnToast?.Invoke(message);

    private static string Normalize(string url)
        => (url ?? string.Empty).TrimStart('/');

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
        => await SendAsync(() => _http.GetAsync(Normalize(url), ct), ct);

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
        => await SendAsync(() => _http.PostAsJsonAsync(Normalize(url), body, ct), ct);

    public async Task PostAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        await SendAsync<AppResult?>(() => _http.PostAsJsonAsync(Normalize(url), body, ct), ct);
    }

    public Task<TResponse?> PostJsonAsync<TResponse, TRequest>(string url, TRequest body, CancellationToken ct = default)
        => PostAsync<TRequest, TResponse>(url, body, ct);

    public async Task PutAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        await SendAsync<AppResult?>(() => _http.PutAsJsonAsync(Normalize(url), body, ct), ct);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
        => await SendAsync(() => _http.PutAsJsonAsync(Normalize(url), body, ct), ct);

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        await SendAsync<AppResult?>(() => _http.DeleteAsync(Normalize(url), ct), ct);
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string url, CancellationToken ct = default)
    {
        return await SendAsync<TResponse>(() => _http.DeleteAsync(Normalize(url), ct), ct);
    }

    private async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> send, CancellationToken ct)
    {
        try
        {
            SetBusy(true);
            var res = await send();
            return await ReadResponseAsync<T>(res, ct);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static async Task<T?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);

        if (TryCreateAppResultFailure<T>(response, await ExtractErrorAsync(response, ct), out var appResult))
            return appResult;

        var message = await ExtractErrorAsync(response, ct);
        throw new HttpRequestException(message.Message ?? "La solicitud falló.", null, response.StatusCode);
    }

    private static bool TryCreateAppResultFailure<T>(HttpResponseMessage response, ErrorPayload error, out T? result)
    {
        var targetType = typeof(T);
        if (targetType == typeof(AppResult))
        {
            result = (T?)(object)new AppResult(false, error.Code ?? response.StatusCode.ToString(), error.Message ?? "La solicitud falló.");
            return true;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(AppResult<>))
        {
            result = (T?)Activator.CreateInstance(
                targetType,
                false,
                null,
                error.Code ?? response.StatusCode.ToString(),
                error.Message ?? "La solicitud falló.");
            return true;
        }

        result = default;
        return false;
    }

    private static async Task<ErrorPayload> ExtractErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var raw = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(raw))
            return new ErrorPayload(response.StatusCode.ToString(), "La solicitud falló.");

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("errorCode", out var errorCode))
            {
                return new ErrorPayload(
                    errorCode.GetString(),
                    root.TryGetProperty("message", out var message) ? message.GetString() : null);
            }

            var title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
            var detail = root.TryGetProperty("detail", out var detailProp) ? detailProp.GetString() : null;
            var validation = TryExtractValidationMessage(root);
            var normalizedMessage = validation ?? detail ?? title ?? raw;
            return new ErrorPayload(response.StatusCode.ToString(), normalizedMessage);
        }
        catch (JsonException)
        {
            return new ErrorPayload(response.StatusCode.ToString(), raw);
        }
    }

    private static string? TryExtractValidationMessage(JsonElement root)
    {
        if (!root.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in errors.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var item in property.Value.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return null;
    }

    private sealed record ErrorPayload(string? Code, string? Message);
}
