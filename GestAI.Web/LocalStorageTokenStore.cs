using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GestAI.Web;

public interface ITokenStore
{
    Task<string?> GetAsync();
    Task SetAsync(string? token);
    Task ClearAsync();
}

public sealed class LocalStorageTokenStore : ITokenStore
{
    private readonly IJSRuntime _js;
    private const string Key = "auth_token";

    public LocalStorageTokenStore(IJSRuntime js) => _js = js;

    public Task<string?> GetAsync()
        => _js.InvokeAsync<string?>("localStorage.getItem", Key).AsTask();

    public Task SetAsync(string? token)
        => _js.InvokeVoidAsync("localStorage.setItem", Key, token ?? string.Empty).AsTask();

    public Task ClearAsync()
        => _js.InvokeVoidAsync("localStorage.removeItem", Key).AsTask();
}

public sealed class AttachTokenAndGuardHandler : DelegatingHandler
{
    private readonly NavigationManager _nav;
    private readonly ITokenStore _tokens;

    public AttachTokenAndGuardHandler(NavigationManager nav, ITokenStore tokens)
    {
        _nav = nav;
        _tokens = tokens;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var token = await _tokens.GetAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            if (IsExpired(token))
            {
                await _tokens.ClearAsync();

                var path = new Uri(_nav.Uri).AbsolutePath;
                if (!path.Equals("/login", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = Uri.EscapeDataString(_nav.Uri);
                    _nav.NavigateTo($"/login?returnUrl={returnUrl}", forceLoad: true);
                }

                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(req, ct);
    }

    private static bool IsExpired(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3) return true;

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);

            if (!doc.RootElement.TryGetProperty("exp", out var expEl))
                return true;

            var exp = expEl.GetInt64();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now >= exp;
        }
        catch
        {
            return true;
        }

        static byte[] Base64UrlDecode(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }
            return Convert.FromBase64String(input);
        }
    }
}
