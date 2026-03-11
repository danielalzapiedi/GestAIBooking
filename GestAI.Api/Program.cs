using AutoMapper;
using FluentValidation;
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
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("all", p => p
        .AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

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

// Interfaces Application
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IUserAccessService, GestAI.Infrastructure.Saas.UserAccessService>();
builder.Services.AddScoped<ISaasPlanService, GestAI.Infrastructure.Saas.SaasPlanService>();
builder.Services.AddScoped<IAuditService, GestAI.Infrastructure.Saas.AuditService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("all");
app.UseAuthentication();
app.UseAuthorization();

app.UseApiExceptionHandling();

app.MapControllers();

// migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

    await DbInitializer.MigrateAndSeedAsync(
        db,
        userMgr,
        roleMgr,
        logger,
        new DbInitializer.SeedOptions(
            AdminEmail: "admin@local.test",
            AdminPassword: "Admin123$",
            PropertyName: "Alma de Lago (Demo)",
            UnitNames: new[] { "Cabaña 1", "Cabaña 2", "Cabaña 3" }
        )
    );
}

app.Run();
