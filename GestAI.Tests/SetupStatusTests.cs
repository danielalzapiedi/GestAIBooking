using GestAI.Application.Abstractions;
using GestAI.Application.Setup;
using GestAI.Domain.Entities;
using GestAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class SetupStatusTests
{
    [Fact]
    public async Task GetSetupStatus_Should_Ignore_Inactive_Default_Property_And_Recommend_Only_Active_Property()
    {
        await using var db = CreateDbContext(nameof(GetSetupStatus_Should_Ignore_Inactive_Default_Property_And_Recommend_Only_Active_Property));
        SeedScenario(db, defaultPropertyId: 2, activePropertyId: 1, inactivePropertyId: 2);

        var handler = new GetSetupStatusQueryHandler(db, new FakeCurrentUser());
        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.Data!.DefaultPropertyId);
        Assert.Equal(1, result.Data.RecommendedPropertyId);
    }

    [Fact]
    public async Task GetSetupStatus_Should_Preserve_Default_Property_When_It_Is_Still_Active()
    {
        await using var db = CreateDbContext(nameof(GetSetupStatus_Should_Preserve_Default_Property_When_It_Is_Still_Active));
        SeedScenario(db, defaultPropertyId: 1, activePropertyId: 1, inactivePropertyId: 2);

        var handler = new GetSetupStatusQueryHandler(db, new FakeCurrentUser());
        var result = await handler.Handle(new GetSetupStatusQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.DefaultPropertyId);
        Assert.Equal(1, result.Data.RecommendedPropertyId);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedScenario(AppDbContext db, int defaultPropertyId, int activePropertyId, int inactivePropertyId)
    {
        var user = new User
        {
            Id = "user-1",
            Email = "owner@test.com",
            UserName = "owner@test.com",
            Nombre = "Owner",
            Apellido = "Test",
            DefaultAccountId = 1,
            DefaultPropertyId = defaultPropertyId,
            IsActive = true
        };

        var account = new Account
        {
            Id = 1,
            Name = "Cuenta",
            OwnerUserId = user.Id,
            IsActive = true
        };

        var activeProperty = new Property
        {
            Id = activePropertyId,
            AccountId = account.Id,
            Account = account,
            Name = "Activa",
            IsActive = true
        };

        var inactiveProperty = new Property
        {
            Id = inactivePropertyId,
            AccountId = account.Id,
            Account = account,
            Name = "Inactiva",
            IsActive = false
        };

        db.Users.Add(user);
        db.Accounts.Add(account);
        db.Properties.AddRange(activeProperty, inactiveProperty);
        db.SaveChanges();
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public string UserId => "user-1";
        public string? Email => "owner@test.com";
        public string? FullName => "Owner Test";
    }
}
