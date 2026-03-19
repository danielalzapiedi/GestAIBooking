using GestAI.Application.Abstractions;
using GestAI.Application.Payments;
using GestAI.Application.Properties;
using GestAI.Application.Quotes;
using GestAI.Domain.Entities;
using GestAI.Infrastructure.Persistence;
using GestAI.Infrastructure.Saas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class PropertyFeatureSettingsTests
{
    [Fact]
    public async Task PropertyFeatureService_Returns_Defaults_When_Setting_Does_Not_Exist()
    {
        await using var db = CreateDbContext(nameof(PropertyFeatureService_Returns_Defaults_When_Setting_Does_Not_Exist));
        SeedProperty(db);

        var service = new PropertyFeatureService(db);
        var settings = await service.GetSettingsAsync(1, CancellationToken.None);

        Assert.True(settings.EnablePayments);
        Assert.True(settings.EnableReports);
        Assert.False(settings.UseSimpleGuestMode);
    }

    [Fact]
    public async Task UpdateFeatureSettingsCommand_Updates_Flags_And_Audits()
    {
        await using var db = CreateDbContext(nameof(UpdateFeatureSettingsCommand_Updates_Flags_And_Audits));
        SeedProperty(db);

        SeedPlan(db, includesReports: true, includesOperations: true);

        var access = new FakeUserAccessService();
        var service = new PropertyFeatureService(db);
        var handler = new PropertyFeatureSettingsHandler(db, new FakeCurrentUser(), access, service, new FakeAuditService());

        var result = await handler.Handle(new UpdatePropertyFeatureSettingsCommand(1, false, true, true, false, false, true, false, true, false, true, true, true, true), CancellationToken.None);

        Assert.True(result.Success);
        Assert.False(result.Data!.EnableHousekeeping);
        Assert.False(result.Data.EnablePayments);
    }

    [Fact]
    public async Task Payments_Are_Blocked_When_Feature_Disabled()
    {
        await using var db = CreateDbContext(nameof(Payments_Are_Blocked_When_Feature_Disabled));
        SeedProperty(db);
        db.PropertyFeatureSettings.Add(new PropertyFeatureSettings { PropertyId = 1, EnablePayments = false });
        db.Bookings.Add(new Booking { Id = 1, PropertyId = 1, UnitId = 1, GuestId = 1, BookingCode = "B-1", CheckInDate = new DateOnly(2026, 1, 1), CheckOutDate = new DateOnly(2026, 1, 2) });
        await db.SaveChangesAsync();

        var handler = new GetPaymentsQueryHandler(db, new FakeCurrentUser(), new PropertyFeatureService(db), new FakeUserAccessService());
        var result = await handler.Handle(new GetPaymentsQuery(1, 1), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("feature_disabled", result.ErrorCode);
    }

    [Fact]
    public async Task UpdateFeatureSettings_Should_Reject_SavedQuotes_Without_Quotes()
    {
        await using var db = CreateDbContext(nameof(UpdateFeatureSettings_Should_Reject_SavedQuotes_Without_Quotes));
        SeedProperty(db);

        SeedPlan(db, includesReports: true, includesOperations: true);

        var access = new FakeUserAccessService();
        var service = new PropertyFeatureService(db);
        var handler = new PropertyFeatureSettingsHandler(db, new FakeCurrentUser(), access, service, new FakeAuditService());

        var result = await handler.Handle(new UpdatePropertyFeatureSettingsCommand(1, true, true, false, true, true, true, true, true, true, true, true, true, false), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("invalid_state", result.ErrorCode);
    }

    [Fact]
    public async Task SavedQuotes_Are_Blocked_When_Feature_Disabled()
    {
        await using var db = CreateDbContext(nameof(SavedQuotes_Are_Blocked_When_Feature_Disabled));
        SeedProperty(db);
        db.PropertyFeatureSettings.Add(new PropertyFeatureSettings { PropertyId = 1, EnableQuotes = true, EnableSavedQuotes = false });
        await db.SaveChangesAsync();

        var handler = new GetQuoteQueryHandler(db, new FakeCurrentUser(), new PropertyFeatureService(db));
        var result = await handler.Handle(new SaveQuoteCommand(1, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 3), 2, 0, "Guest", "g@test.com", "123"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("feature_disabled", result.ErrorCode);
    }

    [Fact]
    public async Task PropertyFeatureService_Returns_Report_State_Without_Module_Authorization()
    {
        await using var db = CreateDbContext(nameof(PropertyFeatureService_Returns_Report_State_Without_Module_Authorization));
        SeedProperty(db);
        db.PropertyFeatureSettings.Add(new PropertyFeatureSettings { PropertyId = 1, EnableReports = true });
        await db.SaveChangesAsync();

        var service = new PropertyFeatureService(db);
        var enabled = await service.IsEnabledAsync(1, GestAI.Domain.Enums.PropertyFeature.Reports, CancellationToken.None);

        Assert.True(enabled);
    }

    [Fact]
    public async Task UpdateFeatureSettings_Should_Reject_Report_When_Module_Is_Not_Available()
    {
        await using var db = CreateDbContext(nameof(UpdateFeatureSettings_Should_Reject_Report_When_Module_Is_Not_Available));
        SeedProperty(db);
        SeedPlan(db, includesReports: false, includesOperations: true);

        var access = new FakeUserAccessService(reportsEnabled: false);
        var service = new PropertyFeatureService(db);
        var handler = new PropertyFeatureSettingsHandler(db, new FakeCurrentUser(), access, service, new FakeAuditService());

        var result = await handler.Handle(new UpdatePropertyFeatureSettingsCommand(1, true, true, true, false, true, true, true, true, true, true, true, true, false), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("module_disabled", result.ErrorCode);
    }

    [Fact]
    public async Task Reports_Query_Should_Return_Forbidden_When_Module_Is_Not_Available()
    {
        await using var db = CreateDbContext(nameof(Reports_Query_Should_Return_Forbidden_When_Module_Is_Not_Available));
        SeedProperty(db);
        db.PropertyFeatureSettings.Add(new PropertyFeatureSettings { PropertyId = 1, EnableReports = true });
        SeedPlan(db, includesReports: false, includesOperations: true);
        await db.SaveChangesAsync();

        var handler = new GestAI.Application.Reports.GetReportsQueryHandler(db, new FakeCurrentUser(), new PropertyFeatureService(db), new FakeUserAccessService(reportsEnabled: false));
        var result = await handler.Handle(new GestAI.Application.Reports.GetReportsQuery(1, new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1), 2026, 1), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("forbidden", result.ErrorCode);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedProperty(AppDbContext db)
    {
        var user = new User { Id = "user-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1 };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = "user-1", IsActive = true };
        var property = new Property { Id = 1, AccountId = 1, Account = account, Name = "Alma", IsActive = true };
        var unit = new Unit { Id = 1, PropertyId = 1, Property = property, Name = "Suite", IsActive = true, BaseRate = 100m };
        var guest = new Guest { Id = 1, PropertyId = 1, Property = property, FullName = "Guest" };

        db.Users.Add(user);
        db.Accounts.Add(account);
        db.Properties.Add(property);
        db.Units.Add(unit);
        db.Guests.Add(guest);
        db.SaveChanges();
    }

    private static void SeedPlan(AppDbContext db, bool includesReports, bool includesOperations)
    {
        db.SaasPlanDefinitions.Add(new SaasPlanDefinition
        {
            Id = 100,
            Code = GestAI.Domain.Enums.SaasPlanCode.Pro,
            Name = "Pro",
            MaxProperties = 3,
            MaxUnits = 20,
            MaxUsers = 5,
            IncludesReports = includesReports,
            IncludesOperations = includesOperations,
            IncludesPublicPortal = true
        });

        db.AccountSubscriptionPlans.Add(new AccountSubscriptionPlan
        {
            AccountId = 1,
            PlanDefinitionId = 100,
            IsActive = true,
            StartedAtUtc = DateTime.UtcNow
        });

        db.SaveChanges();
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public string UserId => "user-1";
        public string? Email => "owner@test.com";
        public string? FullName => "Owner Test";
    }

    private sealed class FakeAuditService : IAuditService
    {
        public Task WriteAsync(int accountId, int? propertyId, string entityName, int? entityId, string action, string summary, CancellationToken ct)
            => Task.CompletedTask;
    }

    private sealed class FakeUserAccessService(bool reportsEnabled = true) : IUserAccessService
    {
        public Task<int?> GetCurrentAccountIdAsync(CancellationToken ct) => Task.FromResult<int?>(1);
        public Task<int?> GetDefaultPropertyIdAsync(CancellationToken ct) => Task.FromResult<int?>(1);
        public Task<bool> HasPropertyAccessAsync(int propertyId, CancellationToken ct) => Task.FromResult(true);
        public Task<bool> HasPropertyModuleAccessAsync(int propertyId, GestAI.Domain.Enums.SaasModule module, CancellationToken ct) => HasModuleAccessAsync(1, module, ct);
        public Task<AccountUser?> GetMembershipAsync(int accountId, CancellationToken ct) => Task.FromResult<AccountUser?>(null);

        public Task<bool> HasModuleAccessAsync(int accountId, GestAI.Domain.Enums.SaasModule module, CancellationToken ct)
            => Task.FromResult(module switch
            {
                GestAI.Domain.Enums.SaasModule.Reports => reportsEnabled,
                _ => true
            });
    }
}
