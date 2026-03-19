using GestAI.Application.Abstractions;
using GestAI.Application.Bookings;
using GestAI.Application.Common;
using GestAI.Application.Common.Pricing;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using GestAI.Infrastructure.Persistence;
using GestAI.Infrastructure.Saas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class CommercialAndBookingTests
{
    [Fact]
    public void DateRange_Overlaps_WhenRangesIntersect()
    {
        var overlaps = DateRange.Overlaps(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 15), new DateOnly(2026, 3, 14), new DateOnly(2026, 3, 18));
        Assert.True(overlaps);
    }

    [Fact]
    public void DateRange_DoesNotOverlap_WhenRangesAreConsecutive()
    {
        var overlaps = DateRange.Overlaps(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 15), new DateOnly(2026, 3, 15), new DateOnly(2026, 3, 18));
        Assert.False(overlaps);
    }

    [Fact]
    public void Promotions_ValidateCommercialRules_ReturnsMinimumStayError()
    {
        var promotion = new Promotion
        {
            Name = "Promo estadía larga",
            IsActive = true,
            MinNights = 4,
            DateFrom = new DateOnly(2026, 1, 1),
            DateTo = new DateOnly(2026, 12, 31)
        };

        var errors = CommercialPricing.ValidatePromotionsAndRules([promotion], new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 12), new DateOnly(2026, 3, 1));

        Assert.Contains(errors, x => x.Contains("estadía mínima", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CommercialPricing_CalculatesWeekendAdjustmentAndPromotion()
    {
        await using var db = CreateDbContext(nameof(CommercialPricing_CalculatesWeekendAdjustmentAndPromotion));
        SeedPricingScenario(db);

        var result = await CommercialPricing.CalculateAsync(db, 1, 1, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 15), 2, 0, CancellationToken.None);

        Assert.Equal(220m, result.BaseAmount);
        Assert.Equal(22m, result.PromotionsAmount);
        Assert.Equal(198m, result.FinalAmount);
        Assert.Equal(59.40m, result.SuggestedDepositAmount);
    }


    [Fact]
    public async Task BookingPricingPreview_ReturnsCalculatedTotal_WhenManualOverrideIsNotUsed()
    {
        await using var db = CreateDbContext(nameof(BookingPricingPreview_ReturnsCalculatedTotal_WhenManualOverrideIsNotUsed));
        SeedPricingScenario(db);

        var handler = new GetBookingPricingPreviewQueryHandler(db, new FakeCurrentUser());
        var result = await handler.Handle(new GetBookingPricingPreviewQuery(1, 1, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 15), 2, 0), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(198m, result.Data!.TotalAmount);
        Assert.Equal(99m, result.Data.SuggestedNightlyRate);
    }

    [Fact]
    public async Task UpsertBooking_UsesCalculatedTotal_WhenManualPriceOverrideIsFalse()
    {
        await using var db = CreateDbContext(nameof(UpsertBooking_UsesCalculatedTotal_WhenManualPriceOverrideIsFalse));
        SeedPricingScenario(db);
        db.Guests.Add(new Guest { Id = 10, PropertyId = 1, Property = db.Properties.First(), FullName = "Guest", IsActive = true });
        await db.SaveChangesAsync();

        var features = new FakePropertyFeatureService();
        var handler = new UpsertBookingCommandHandler(db, new FakeCurrentUser(), features);
        var result = await handler.Handle(new UpsertBookingCommand(1, null, 1, 10, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 15), 2, 0, 999m, null), CancellationToken.None);

        Assert.True(result.Success);
        var booking = await db.Bookings.FirstAsync(x => x.Id == result.Data);
        Assert.Equal(198m, booking.TotalAmount);
        Assert.False(booking.ManualPriceOverride);
    }

    [Fact]
    public async Task UpsertBooking_PreservesManualTotal_WhenManualPriceOverrideIsTrue()
    {
        await using var db = CreateDbContext(nameof(UpsertBooking_PreservesManualTotal_WhenManualPriceOverrideIsTrue));
        SeedPricingScenario(db);
        db.Guests.Add(new Guest { Id = 11, PropertyId = 1, Property = db.Properties.First(), FullName = "Guest", IsActive = true });
        await db.SaveChangesAsync();

        var features = new FakePropertyFeatureService();
        var handler = new UpsertBookingCommandHandler(db, new FakeCurrentUser(), features);
        var result = await handler.Handle(new UpsertBookingCommand(1, null, 1, 11, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 15), 2, 0, 250m, null, ManualPriceOverride: true, ConfirmManualPriceChange: true), CancellationToken.None);

        Assert.True(result.Success);
        var booking = await db.Bookings.FirstAsync(x => x.Id == result.Data);
        Assert.Equal(250m, booking.TotalAmount);
        Assert.True(booking.ManualPriceOverride);
    }

    [Fact]
    public async Task UpsertBooking_Applies_Promotions_Based_On_Property_Feature_Settings_Only()
    {
        await using var db = CreateDbContext(nameof(UpsertBooking_Applies_Promotions_Based_On_Property_Feature_Settings_Only));
        SeedPricingScenario(db);
        db.PropertyFeatureSettings.Add(new PropertyFeatureSettings { PropertyId = 1, EnablePromotions = true });
        db.Guests.Add(new Guest { Id = 12, PropertyId = 1, Property = db.Properties.First(), FullName = "Guest", IsActive = true });
        await db.SaveChangesAsync();

        var handler = new UpsertBookingCommandHandler(db, new FakeCurrentUser(), new PropertyFeatureService(db));
        var result = await handler.Handle(new UpsertBookingCommand(1, null, 1, 12, new DateOnly(2026, 3, 13), new DateOnly(2026, 3, 15), 2, 0, 999m, null), CancellationToken.None);

        Assert.True(result.Success);
        var booking = await db.Bookings.FirstAsync(x => x.Id == result.Data);
        Assert.Equal(22m, booking.PromotionsAmount);
        Assert.Equal(198m, booking.TotalAmount);
    }

    [Fact]
    public async Task ChangeBookingStatus_UpdatesBookingAndUnitOperationalState()
    {
        await using var db = CreateDbContext(nameof(ChangeBookingStatus_UpdatesBookingAndUnitOperationalState));
        SeedBookingScenario(db);

        var handler = new ChangeBookingStatusCommandHandler(db, new FakeCurrentUser());
        var response = await handler.Handle(new ChangeBookingStatusCommand(1, 1, BookingStatus.CheckedOut), CancellationToken.None);

        Assert.True(response.Success);

        var booking = await db.Bookings.Include(x => x.Unit).FirstAsync(x => x.Id == 1);
        Assert.Equal(BookingStatus.CheckedOut, booking.Status);
        Assert.Equal(BookingOperationalStatus.CheckedOut, booking.OperationalStatus);
        Assert.Equal(UnitOperationalStatus.PendingCleaning, booking.Unit.OperationalStatus);
    }

    [Fact]
    public async Task Checkout_Generates_Cleaning_Task_Based_On_Property_Feature_Settings_Only()
    {
        await using var db = CreateDbContext(nameof(Checkout_Generates_Cleaning_Task_Based_On_Property_Feature_Settings_Only));
        SeedBookingScenario(db);
        db.PropertyFeatureSettings.Add(new PropertyFeatureSettings { PropertyId = 1, EnableHousekeeping = true });
        await db.SaveChangesAsync();

        var handler = new UpsertBookingCommandHandler(db, new FakeCurrentUser(), new PropertyFeatureService(db));
        var result = await handler.Handle(new CheckInOutCommand(1, 1, false, null, "Checkout"), CancellationToken.None);

        Assert.True(result.Success);

        var booking = await db.Bookings.Include(x => x.Unit).FirstAsync(x => x.Id == 1);
        var task = await db.OperationalTasks.SingleAsync();
        Assert.Equal(UnitOperationalStatus.PendingCleaning, booking.Unit.OperationalStatus);
        Assert.Equal(OperationalTaskType.Cleaning, task.Type);
        Assert.Equal(1, task.BookingId);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedPricingScenario(AppDbContext db)
    {
        var user = new User { Id = "user-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1 };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = "user-1", IsActive = true };
        var property = new Property { Id = 1, AccountId = 1, Account = account, Name = "Alma", IsActive = true, DefaultDepositPercentage = 30m };
        var unit = new Unit { Id = 1, PropertyId = 1, Property = property, Name = "Suite", IsActive = true, BaseRate = 100m, TotalCapacity = 2, CapacityAdults = 2, CapacityChildren = 0 };
        var ratePlan = new RatePlan
        {
            Id = 1,
            PropertyId = 1,
            Property = property,
            UnitId = 1,
            Unit = unit,
            Name = "Base",
            BaseNightlyRate = 100m,
            IsActive = true,
            WeekendAdjustmentEnabled = true,
            WeekendAdjustmentType = RateAdjustmentType.Percentage,
            WeekendAdjustmentValue = 20m
        };
        var promotion = new Promotion
        {
            Id = 1,
            PropertyId = 1,
            Property = property,
            Name = "Promo 10%",
            IsActive = true,
            IsDeleted = false,
            IsCumulative = false,
            Priority = 1,
            DateFrom = new DateOnly(2026, 1, 1),
            DateTo = new DateOnly(2026, 12, 31),
            ValueType = DiscountValueType.Percentage,
            Scope = PromotionScope.EntireStay,
            Value = 10m
        };

        db.Users.Add(user);
        db.Accounts.Add(account);
        db.Properties.Add(property);
        db.Units.Add(unit);
        db.RatePlans.Add(ratePlan);
        db.Promotions.Add(promotion);
        db.SaveChanges();
    }

    private static void SeedBookingScenario(AppDbContext db)
    {
        var user = new User { Id = "user-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1 };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = "user-1", IsActive = true };
        var property = new Property { Id = 1, AccountId = 1, Account = account, Name = "Alma", IsActive = true };
        var unit = new Unit { Id = 1, PropertyId = 1, Property = property, Name = "Suite", IsActive = true, BaseRate = 100m, OperationalStatus = UnitOperationalStatus.Occupied };
        var guest = new Guest { Id = 1, PropertyId = 1, Property = property, FullName = "Huésped Demo" };
        var booking = new Booking
        {
            Id = 1,
            PropertyId = 1,
            Property = property,
            UnitId = 1,
            Unit = unit,
            GuestId = 1,
            Guest = guest,
            BookingCode = "RSV-001",
            CheckInDate = new DateOnly(2026, 3, 10),
            CheckOutDate = new DateOnly(2026, 3, 12),
            Status = BookingStatus.CheckedIn,
            OperationalStatus = BookingOperationalStatus.CheckedIn,
            TotalAmount = 200m
        };

        db.Users.Add(user);
        db.Accounts.Add(account);
        db.Properties.Add(property);
        db.Units.Add(unit);
        db.Guests.Add(guest);
        db.Bookings.Add(booking);
        db.SaveChanges();
    }


    private sealed class FakePropertyFeatureService : IPropertyFeatureService
    {
        public Task<PropertyFeatureSettings> GetSettingsAsync(int propertyId, CancellationToken ct)
            => Task.FromResult(new PropertyFeatureSettings { PropertyId = propertyId, EnablePromotions = true, EnableAdvancedRates = true });

        public Task<bool> IsEnabledAsync(int propertyId, PropertyFeature feature, CancellationToken ct)
            => Task.FromResult(true);
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public string UserId => "user-1";
        public string? Email => "owner@test.com";
        public string? FullName => "Owner Test";
    }
}
