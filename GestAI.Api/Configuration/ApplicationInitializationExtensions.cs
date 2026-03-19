using GestAI.Domain.Entities;
using GestAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GestAI.Api.Configuration;

public static class ApplicationInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseBootstrap");
        var options = scope.ServiceProvider
            .GetRequiredService<IOptions<DatabaseBootstrapOptions>>()
            .Value;

        if (!options.ApplyMigrations && !options.RunDemoSeed)
        {
            logger.LogInformation("Database bootstrap disabled by configuration.");
            return;
        }

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (options.ApplyMigrations)
        {
            await DbInitializer.ApplyMigrationsAsync(db, logger, cancellationToken);
        }

        if (!options.RunDemoSeed)
        {
            logger.LogInformation("Demo seed disabled by configuration.");
            return;
        }

        var seed = options.DemoSeed;
        if (string.IsNullOrWhiteSpace(seed.AdminEmail) || string.IsNullOrWhiteSpace(seed.AdminPassword))
        {
            logger.LogWarning("Demo seed skipped because admin credentials were not configured.");
            return;
        }

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.SeedDemoDataAsync(
            db,
            userMgr,
            roleMgr,
            logger,
            new DbInitializer.SeedOptions(
                seed.AdminEmail,
                seed.AdminPassword,
                seed.PropertyName,
                seed.UnitNames),
            cancellationToken);
    }
}
