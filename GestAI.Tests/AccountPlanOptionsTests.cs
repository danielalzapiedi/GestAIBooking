using GestAI.Application.Abstractions;
using GestAI.Application.Saas;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using GestAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class AccountPlanOptionsTests
{
    [Fact]
    public async Task GetAccountPlanOptions_Should_Return_Ordered_Plans_When_User_Can_Manage_Account()
    {
        await using var db = CreateDbContext(nameof(GetAccountPlanOptions_Should_Return_Ordered_Plans_When_User_Can_Manage_Account));
        SeedPlans(db);

        var handler = new GetAccountPlanOptionsQueryHandler(db, new FakeUserAccessService());
        var result = await handler.Handle(new GetAccountPlanOptionsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Collection(result.Data!,
            starter =>
            {
                Assert.Equal("Starter", starter.Name);
                Assert.False(starter.IncludesReports);
            },
            pro => Assert.Equal("Pro", pro.Name),
            manager => Assert.Equal("Manager", manager.Name));
    }

    [Fact]
    public async Task UpdateAccount_Should_Fail_When_Selected_Plan_Does_Not_Exist()
    {
        await using var db = CreateDbContext(nameof(UpdateAccount_Should_Fail_When_Selected_Plan_Does_Not_Exist));
        SeedAccount(db);
        db.SaasPlanDefinitions.Add(new SaasPlanDefinition
        {
            Id = 100,
            Code = SaasPlanCode.Pro,
            Name = "Pro",
            MaxProperties = 3,
            MaxUnits = 20,
            MaxUsers = 5,
            IncludesReports = true,
            IncludesOperations = true,
            IncludesPublicPortal = true
        });
        db.AccountSubscriptionPlans.Add(new AccountSubscriptionPlan
        {
            AccountId = 1,
            PlanDefinitionId = 100,
            IsActive = true,
            StartedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new UpdateAccountCommandHandler(db, new FakeUserAccessService(), new FakeAuditService());
        var result = await handler.Handle(new UpdateAccountCommand("Cuenta actualizada", 999), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("plan_not_found", result.ErrorCode);
        Assert.True(await db.AccountSubscriptionPlans.AnyAsync(x => x.PlanDefinitionId == 100 && x.IsActive));
        Assert.False(await db.AccountSubscriptionPlans.AnyAsync(x => x.PlanDefinitionId == 999));
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedPlans(AppDbContext db)
    {
        SeedAccount(db);
        db.SaasPlanDefinitions.AddRange(
            new SaasPlanDefinition
            {
                Id = 1,
                Code = SaasPlanCode.Manager,
                Name = "Manager",
                MaxProperties = 10,
                MaxUnits = 100,
                MaxUsers = 20,
                IncludesReports = true,
                IncludesOperations = true,
                IncludesPublicPortal = true
            },
            new SaasPlanDefinition
            {
                Id = 2,
                Code = SaasPlanCode.Starter,
                Name = "Starter",
                MaxProperties = 1,
                MaxUnits = 5,
                MaxUsers = 2,
                IncludesReports = false,
                IncludesOperations = false,
                IncludesPublicPortal = false
            },
            new SaasPlanDefinition
            {
                Id = 3,
                Code = SaasPlanCode.Pro,
                Name = "Pro",
                MaxProperties = 3,
                MaxUnits = 20,
                MaxUsers = 5,
                IncludesReports = true,
                IncludesOperations = true,
                IncludesPublicPortal = true
            });
        db.SaveChanges();
    }

    private static void SeedAccount(AppDbContext db)
    {
        var owner = new User { Id = "owner-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1, IsActive = true };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = owner.Id, IsActive = true };

        db.Users.Add(owner);
        db.Accounts.Add(account);
        db.SaveChanges();
    }

    private sealed class FakeUserAccessService : IUserAccessService
    {
        public Task<int?> GetCurrentAccountIdAsync(CancellationToken ct) => Task.FromResult<int?>(1);
        public Task<int?> GetDefaultPropertyIdAsync(CancellationToken ct) => Task.FromResult<int?>(1);
        public Task<bool> HasPropertyAccessAsync(int propertyId, CancellationToken ct) => Task.FromResult(true);
        public Task<bool> HasPropertyModuleAccessAsync(int propertyId, SaasModule module, CancellationToken ct) => Task.FromResult(true);
        public Task<AccountUser?> GetMembershipAsync(int accountId, CancellationToken ct) => Task.FromResult<AccountUser?>(null);
        public Task<bool> HasModuleAccessAsync(int accountId, SaasModule module, CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class FakeAuditService : IAuditService
    {
        public Task WriteAsync(int accountId, int? propertyId, string entityName, int? entityId, string action, string summary, CancellationToken ct)
            => Task.CompletedTask;
    }
}
