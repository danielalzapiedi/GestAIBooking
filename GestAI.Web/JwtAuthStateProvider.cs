using System.Security.Claims;
using System.Text.Json;
using GestAI.Web.Service;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestAI.Web;

public class JwtAuthStateProvider(LocalStorageService storage) : AuthenticationStateProvider
{
    private const string TokenKey = "auth_token";

    public async Task SetTokenAsync(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token)) await storage.SetAsync(TokenKey, token);
        else await storage.RemoveAsync(TokenKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetTokenAsync() => await storage.GetAsync(TokenKey);

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await storage.GetAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        try
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            await storage.RemoveAsync(TokenKey);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)!;

        var claims = new List<Claim>();
        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value is JsonElement el && el.ValueKind == JsonValueKind.Array && kvp.Key == "role")
            {
                foreach (var r in el.EnumerateArray())
                    claims.Add(new Claim(ClaimTypes.Role, r.GetString()!));
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? ""));
            }
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));
    }
}
