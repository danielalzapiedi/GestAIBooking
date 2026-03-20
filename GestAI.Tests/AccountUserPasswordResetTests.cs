using GestAI.Application.Abstractions;
using GestAI.Application.Saas;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using GestAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class AccountUserPasswordResetTests
{
    [Fact]
    public async Task UpsertAccountUser_Should_ResetPassword_When_Update_Includes_NewPassword()
    {
        await using var db = CreateDbContext(nameof(UpsertAccountUser_Should_ResetPassword_When_Update_Includes_NewPassword));
        SeedScenario(db);

        var identity = new FakeIdentityService();
        var handler = new UpsertAccountUserCommandHandler(
            db,
            identity,
            new FakeUserAccessService(),
            new FakePlanService(),
            new FakeAuditService());

        var command = new UpsertAccountUserCommand("user-2", "Reception", "Updated", "reception@test.com", true, InternalUserRole.Reception, 1, "TmpReset123!");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(identity.ResetPasswordCalled);
        Assert.Equal("user-2", identity.ResetPasswordUserId);
        Assert.Equal("TmpReset123!", identity.ResetPasswordValue);
    }

    [Fact]
    public async Task UpsertAccountUser_Should_Fail_When_Update_Uses_Email_Already_Assigned_To_Other_User()
    {
        await using var db = CreateDbContext(nameof(UpsertAccountUser_Should_Fail_When_Update_Uses_Email_Already_Assigned_To_Other_User));
        SeedScenario(db);

        var identity = new FakeIdentityService();
        identity.RegisterExistingEmail("owner@test.com", "owner-1");

        var handler = new UpsertAccountUserCommandHandler(
            db,
            identity,
            new FakeUserAccessService(),
            new FakePlanService(),
            new FakeAuditService());

        var command = new UpsertAccountUserCommand("user-2", "Reception", "Updated", "owner@test.com", true, InternalUserRole.Reception, 1, null);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("email_exists", result.ErrorCode);
        Assert.False(identity.ResetPasswordCalled);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedScenario(AppDbContext db)
    {
        var owner = new User { Id = "owner-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1, IsActive = true };
        var user = new User { Id = "user-2", Email = "reception@test.com", UserName = "reception@test.com", Nombre = "Reception", Apellido = "Test", DefaultAccountId = 1, DefaultPropertyId = 1, IsActive = true };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = owner.Id, IsActive = true };
        var property = new Property { Id = 1, AccountId = 1, Account = account, Name = "Alma", IsActive = true };
        var membership = new AccountUser { Id = 1, AccountId = 1, Account = account, UserId = user.Id, User = user, Role = InternalUserRole.Reception, IsActive = true };

        db.Users.AddRange(owner, user);
        db.Accounts.Add(account);
        db.Properties.Add(property);
        db.AccountUsers.Add(membership);
        db.SaveChanges();
    }

    private sealed class FakeIdentityService : IIdentityService
    {
        private readonly Dictionary<string, string> _emails = new(StringComparer.OrdinalIgnoreCase);

        public bool ResetPasswordCalled { get; private set; }
        public string? ResetPasswordUserId { get; private set; }
        public string? ResetPasswordValue { get; private set; }

        public void RegisterExistingEmail(string email, string userId)
            => _emails[email] = userId;

        public Task<(bool Success, string? UserId, string? Error)> CreateUserIfNotExistsAsync(string email, string password, CancellationToken ct)
            => Task.FromResult((true, "new-user", (string?)null));

        public Task<(bool Success, string? UserId, string? Error)> CreateUserIfNotExistsAsync(string email, string password, CancellationToken ct, string firstName, string lastName, bool isActive, int? defaultPropertyId, int defaultAccountId)
            => Task.FromResult((true, "new-user", (string?)null));

        public Task<(bool Success, string? UserId, string? Error)> FindUserIdByEmailAsync(string email, CancellationToken ct)
            => Task.FromResult(_emails.TryGetValue(email, out var userId)
                ? (true, userId, (string?)null)
                : (true, (string?)null, (string?)null));

        public Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string newPassword, CancellationToken ct)
        {
            ResetPasswordCalled = true;
            ResetPasswordUserId = userId;
            ResetPasswordValue = newPassword;
            return Task.FromResult((true, (string?)null));
        }
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

    private sealed class FakePlanService : ISaasPlanService
    {
        public Task<SaasPlanDefinition?> GetActivePlanAsync(int accountId, CancellationToken ct) => Task.FromResult<SaasPlanDefinition?>(null);
        public Task<(bool Success, string? ErrorCode, string? Message)> ValidatePropertyCreationAsync(int accountId, CancellationToken ct) => Task.FromResult((true, (string?)null, (string?)null));
        public Task<(bool Success, string? ErrorCode, string? Message)> ValidateUnitCreationAsync(int accountId, CancellationToken ct) => Task.FromResult((true, (string?)null, (string?)null));
        public Task<(bool Success, string? ErrorCode, string? Message)> ValidateUserCreationAsync(int accountId, CancellationToken ct) => Task.FromResult((true, (string?)null, (string?)null));
    }

    private sealed class FakeAuditService : IAuditService
    {
        public Task WriteAsync(int accountId, int? propertyId, string entityName, int? entityId, string action, string summary, CancellationToken ct)
            => Task.CompletedTask;
    }
}
