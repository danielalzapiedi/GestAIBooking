using GestAI.Application;
using GestAI.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestAI.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=GestAIBookingDb;Trusted_Connection=True;TrustServerCertificate=True";
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(cs);
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        return services;
    }
}
