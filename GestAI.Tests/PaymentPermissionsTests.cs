using GestAI.Application.Abstractions;
using GestAI.Application.Payments;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using GestAI.Infrastructure.Persistence;
using GestAI.Infrastructure.Saas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class PaymentPermissionsTests
{
    [Fact]
    public async Task DeletePayment_Allows_Active_Account_User_With_Payments_Module()
    {
        await using var db = CreateDbContext(nameof(DeletePayment_Allows_Active_Account_User_With_Payments_Module));
        SeedScenario(db);

        var handler = new DeletePaymentCommandHandler(
            db,
            new FakeCurrentUser("reception-1", "reception@test.com"),
            new PropertyFeatureService(db),
            new FakeUserAccessService());

        var result = await handler.Handle(new DeletePaymentCommand(1, 1), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(db.Payments);
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
        var owner = new User { Id = "owner-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1 };
        var receptionist = new User { Id = "reception-1", Email = "reception@test.com", UserName = "reception@test.com", Nombre = "Reception", Apellido = "Test", DefaultAccountId = 1 };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = owner.Id, IsActive = true };
        var property = new Property { Id = 1, AccountId = 1, Account = account, Name = "Alma", IsActive = true };
        var unit = new Unit { Id = 1, PropertyId = 1, Property = property, Name = "Suite", IsActive = true };
        var guest = new Guest { Id = 1, PropertyId = 1, Property = property, FullName = "Guest", IsActive = true };
        var booking = new Booking { Id = 1, PropertyId = 1, Property = property, UnitId = 1, Unit = unit, GuestId = 1, Guest = guest, BookingCode = "B-1", CheckInDate = new DateOnly(2026, 3, 10), CheckOutDate = new DateOnly(2026, 3, 12), Status = BookingStatus.Confirmed };
        var payment = new Payment { Id = 1, PropertyId = 1, Property = property, BookingId = 1, Booking = booking, Amount = 150m, Status = PaymentStatus.Paid };
        var membership = new AccountUser { Id = 1, AccountId = 1, Account = account, UserId = receptionist.Id, User = receptionist, Role = InternalUserRole.Reception, IsActive = true };
        var featureSettings = new PropertyFeatureSettings { PropertyId = 1, Property = property, EnablePayments = true };

        db.Users.AddRange(owner, receptionist);
        db.Accounts.Add(account);
        db.AccountUsers.Add(membership);
        db.Properties.Add(property);
        db.PropertyFeatureSettings.Add(featureSettings);
        db.Units.Add(unit);
        db.Guests.Add(guest);
        db.Bookings.Add(booking);
        db.Payments.Add(payment);
        db.SaveChanges();
    }

    private sealed class FakeCurrentUser(string userId, string? email) : ICurrentUser
    {
        public string UserId { get; } = userId;
        public string? Email { get; } = email;
        public string? FullName => "Reception Test";
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
}
