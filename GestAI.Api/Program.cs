using AutoMapper;
using FluentValidation;
using GestAI.Api.Configuration;
using GestAI.Api.Middleware;
using GestAI.Application;
using GestAI.Application.Abstractions;
using GestAI.Application.Behaviors;
using GestAI.Application.Mapping;
using GestAI.Application.Security;
using GestAI.Domain.Entities;
using GestAI.Infrastructure;
using GestAI.Infrastructure.Identity;
using GestAI.Infrastructure.Payments;
using GestAI.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var corsOptions = builder.Configuration
    .GetSection(ApiCorsOptions.SectionName)
    .Get<ApiCorsOptions>() ?? new ApiCorsOptions();
var allowedOrigins = corsOptions.AllowedOrigins
    .Where(static origin => !string.IsNullOrWhiteSpace(origin))
    .Select(static origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.Configure<ApiCorsOptions>(builder.Configuration.GetSection(ApiCorsOptions.SectionName));
builder.Services.Configure<DatabaseBootstrapOptions>(builder.Configuration.GetSection(DatabaseBootstrapOptions.SectionName));

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy(ApiCorsOptions.SectionName, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddIdentityCore<User>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GestAI.Application.AssemblyMarker).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(GestAI.Application.AssemblyMarker).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(MappingProfile).Assembly);
});

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPayPal(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<GestAI.Infrastructure.Calendars.ExternalCalendarAutoSyncBackgroundService>();

// Interfaces Application
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IUserAccessService, GestAI.Infrastructure.Saas.UserAccessService>();
builder.Services.AddScoped<ISaasPlanService, GestAI.Infrastructure.Saas.SaasPlanService>();
builder.Services.AddScoped<IAuditService, GestAI.Infrastructure.Saas.AuditService>();
builder.Services.AddScoped<IPropertyFeatureService, GestAI.Infrastructure.Saas.PropertyFeatureService>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

app.UseSwagger();
app.UseSwaggerUI();

if (allowedOrigins.Length > 0)
{
    app.UseCors(ApiCorsOptions.SectionName);
    logger.LogInformation("CORS enabled for {OriginCount} configured origin(s).", allowedOrigins.Length);
}
else
{
    logger.LogInformation("CORS middleware disabled because no allowed origins were configured.");
}

app.UseAuthentication();
app.UseAuthorization();

app.UseApiExceptionHandling();

app.MapControllers();

await app.InitializeDatabaseAsync();

app.Run();
