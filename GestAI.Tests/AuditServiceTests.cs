using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using GestAI.Infrastructure.Persistence;
using GestAI.Infrastructure.Saas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GestAI.Tests;

public class AuditServiceTests
{
    [Fact]
    public async Task WriteAsync_Should_Save_Authenticated_User_When_Available()
    {
        await using var db = CreateDbContext(nameof(WriteAsync_Should_Save_Authenticated_User_When_Available));
        var service = new AuditService(db, new FakeCurrentUser("user-123", "owner@test.com", "Owner Test"), NullLogger<AuditService>.Instance);

        await service.WriteAsync(1, 2, "Booking", 10, "created", "Reserva creada", CancellationToken.None);

        var log = await db.AuditLogs.SingleAsync();
        Assert.Equal("user-123", log.UserId);
        Assert.Equal("Owner Test", log.UserName);
    }

    [Fact]
    public async Task WriteAsync_Should_Save_System_User_When_No_Authenticated_User()
    {
        await using var db = CreateDbContext(nameof(WriteAsync_Should_Save_System_User_When_No_Authenticated_User));
        var service = new AuditService(db, new ThrowingCurrentUser(), NullLogger<AuditService>.Instance);

        await service.WriteAsync(1, 2, "ExternalCalendarSync", 5, "Sync", "Auto sync", CancellationToken.None);

        var log = await db.AuditLogs.SingleAsync();
        Assert.Null(log.UserId);
        Assert.Equal("SYSTEM", log.UserName);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeCurrentUser(string userId, string? email, string? fullName) : ICurrentUser
    {
        public string UserId { get; } = userId;
        public string? Email { get; } = email;
        public string? FullName { get; } = fullName;
    }

    private sealed class ThrowingCurrentUser : ICurrentUser
    {
        public string UserId => throw new InvalidOperationException("Usuario no autenticado.");
        public string? Email => null;
        public string? FullName => null;
    }
}
