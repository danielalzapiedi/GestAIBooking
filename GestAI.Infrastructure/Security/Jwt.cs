using System;
using System.Collections.Generic;

namespace GestAI.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "GestAI.Api";
    public string Audience { get; set; } = "GestAI.Web";
    public string Key { get; set; } = "dev-secret-please-change";
    public int ExpirationMinutes { get; set; } = 120;
}

public interface ITokenService
{
    string Create(
        string userId,
        string email,
        string firstName,
        string lastName,
        IEnumerable<string> roles,
        DateTime nowUtc,
        DateTime expiresUtc);
}
