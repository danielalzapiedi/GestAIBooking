using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GestAI.Infrastructure.Security;

public class TokenService(JwtSettings settings) : ITokenService
{
    public string Create(
        string userId,
        string email,
        string firstName,
        string lastName,
        IEnumerable<string> roles,
        DateTime nowUtc,
        DateTime expiresUtc)
    {
        var displayName = $"{firstName} {lastName}".Trim();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),

            // Email (doble, para compatibilidad)
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),

            // Nombre para UI
            new(ClaimTypes.Name, displayName),
            new(JwtRegisteredClaimNames.Name, displayName),

            // Opcional: por si querés mostrar/usar por separado
            new("given_name", firstName ?? ""),
            new("family_name", lastName ?? "")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresUtc,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
