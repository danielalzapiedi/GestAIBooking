using System.Net;
using System.Net.Http.Json;

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
    {
        try
        {
            SetBusy(true);
            return await _http.GetFromJsonAsync<T>(Normalize(url), ct);
        }
        finally
        {
            SetBusy(false);
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            SetBusy(true);
            var res = await _http.PostAsJsonAsync(Normalize(url), body, ct);
            res.EnsureSuccessStatusCode();

            if (res.StatusCode == HttpStatusCode.NoContent)
                return default;

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        finally
        {
            SetBusy(false);
        }
    }

    public async Task PostAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            SetBusy(true);
            var res = await _http.PostAsJsonAsync(Normalize(url), body, ct);
            res.EnsureSuccessStatusCode();
        }
        finally
        {
            SetBusy(false);
        }
    }

    public Task<TResponse?> PostJsonAsync<TResponse, TRequest>(string url, TRequest body, CancellationToken ct = default)
        => PostAsync<TRequest, TResponse>(url, body, ct);

    public async Task PutAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            SetBusy(true);
            var res = await _http.PutAsJsonAsync(Normalize(url), body, ct);
            res.EnsureSuccessStatusCode();
        }
        finally
        {
            SetBusy(false);
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            SetBusy(true);
            var res = await _http.PutAsJsonAsync(Normalize(url), body, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        finally
        {
            SetBusy(false);
        }
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        try
        {
            SetBusy(true);
            var res = await _http.DeleteAsync(Normalize(url), ct);
            res.EnsureSuccessStatusCode();
        }
        finally
        {
            SetBusy(false);
        }
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(string url, CancellationToken ct = default)
    {
        try
        {
            SetBusy(true);
            var res = await _http.DeleteAsync(Normalize(url), ct);
            res.EnsureSuccessStatusCode();

            if (res.StatusCode == HttpStatusCode.NoContent)
                return default;

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        finally
        {
            SetBusy(false);
        }
    }
}
