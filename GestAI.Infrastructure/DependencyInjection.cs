using System.Text;
using GestAI.Infrastructure.Security;
using GestAI.Infrastructure.Ai;
using GestAI.Infrastructure.Calendars;
using GestAI.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GestAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var jwt = new JwtSettings();
        config.GetSection("Jwt").Bind(jwt);
        services.AddSingleton(jwt);
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IBookingAssistantService, MockBookingAssistantService>();
        services.AddScoped<IQuoteSuggestionService, MockQuoteSuggestionService>();
        services.AddHttpClient<IIcsCalendarService, IcsCalendarService>();
        services.AddScoped<IExternalCalendarSyncService, ExternalCalendarSyncService>();
        services.Configure<ExternalCalendarAutoSyncOptions>(config.GetSection(ExternalCalendarAutoSyncOptions.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Bearer";
            options.DefaultChallengeScheme = "Bearer";
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
            };
        });

        return services;
    }
}
