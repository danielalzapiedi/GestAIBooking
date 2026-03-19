using GestAI.Application.Abstractions;
using GestAI.Application.Bookings;
using GestAI.Application.Common;
using GestAI.Application.Quotes;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using GestAI.Infrastructure.Calendars;
using GestAI.Infrastructure.Persistence;
using GestAI.Infrastructure.Saas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestAI.Tests;

public class ExternalCalendarTests
{
    [Fact]
    public async Task Quote_Should_Not_Return_Unit_When_External_Event_Overlaps()
    {
        await using var db = CreateDbContext(nameof(Quote_Should_Not_Return_Unit_When_External_Event_Overlaps));
        SeedAvailabilityScenario(db);

        var features = new PropertyFeatureService(db);
        var handler = new GetQuoteQueryHandler(db, new FakeCurrentUser(), features);
        var result = await handler.Handle(new GetQuoteQuery(1, 1, new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 12), 2, 0), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.AvailableUnits);
    }

    [Fact]
    public async Task UpsertBooking_Should_Fail_When_External_Event_Overlaps()
    {
        await using var db = CreateDbContext(nameof(UpsertBooking_Should_Fail_When_External_Event_Overlaps));
        SeedAvailabilityScenario(db);
        db.Guests.Add(new Guest { Id = 10, PropertyId = 1, Property = db.Properties.First(), FullName = "Guest" , IsActive = true});
        await db.SaveChangesAsync();

        var features = new PropertyFeatureService(db);
        var handler = new UpsertBookingCommandHandler(db, new FakeCurrentUser(), features);
        var result = await handler.Handle(new UpsertBookingCommand(1, null, 1, 10, new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 12), 2, 0, 100m, null), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("overlap_external", result.ErrorCode);
    }

    [Fact]
    public void IcsCalendarService_Should_Parse_All_Day_Events()
    {
        var service = new IcsCalendarService(new HttpClient(new StubHandler(@"BEGIN:VCALENDAR
VERSION:2.0
BEGIN:VEVENT
UID:test-1
DTSTART;VALUE=DATE:20260310
DTEND;VALUE=DATE:20260312
SUMMARY:Reserva Booking
END:VEVENT
END:VCALENDAR")));

        var events = service.LoadAsync("https://example.com/test.ics", CancellationToken.None).GetAwaiter().GetResult();

        Assert.Single(events);
        Assert.Equal(new DateOnly(2026, 3, 10), events[0].StartDate);
        Assert.Equal(new DateOnly(2026, 3, 12), events[0].EndDate);
        Assert.Equal("Reserva Booking", events[0].Summary);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    private static void SeedAvailabilityScenario(AppDbContext db)
    {
        var user = new User { Id = "user-1", Email = "owner@test.com", UserName = "owner@test.com", Nombre = "Owner", Apellido = "Test", DefaultAccountId = 1 };
        var account = new Account { Id = 1, Name = "Cuenta", OwnerUserId = "user-1", IsActive = true };
        var property = new Property { Id = 1, AccountId = 1, Account = account, Name = "Alma", IsActive = true, DefaultDepositPercentage = 30m };
        var unit = new Unit { Id = 1, PropertyId = 1, Property = property, Name = "Suite", IsActive = true, BaseRate = 100m, TotalCapacity = 2, CapacityAdults = 2, CapacityChildren = 0 };
        var ratePlan = new RatePlan { Id = 1, PropertyId = 1, Property = property, UnitId = 1, Unit = unit, Name = "Base", BaseNightlyRate = 100m, IsActive = true };
        var connection = new ExternalChannelConnection { Id = 1, PropertyId = 1, Property = property, UnitId = 1, Unit = unit, ChannelType = ExternalChannelType.Booking, DisplayName = "Booking", ImportCalendarUrl = "https://example.com/calendar.ics", ExportToken = "token", IsActive = true };
        var externalEvent = new ExternalCalendarEvent { Id = 1, ExternalChannelConnectionId = 1, ExternalChannelConnection = connection, PropertyId = 1, Property = property, UnitId = 1, Unit = unit, ExternalEventUid = "ext-1", StartDate = new DateOnly(2026, 3, 10), EndDate = new DateOnly(2026, 3, 12), Summary = "Reserva externa", SourceChannel = ExternalChannelType.Booking };

        db.Users.Add(user);
        db.Accounts.Add(account);
        db.Properties.Add(property);
        db.Units.Add(unit);
        db.RatePlans.Add(ratePlan);
        db.ExternalChannelConnections.Add(connection);
        db.ExternalCalendarEvents.Add(externalEvent);
        db.SaveChanges();
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public string UserId => "user-1";
        public string? Email => "owner@test.com";
        public string? FullName => "Owner Test";
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

    private sealed class StubHandler(string responseContent) : HttpMessageHandler
    {
        private readonly string _responseContent = responseContent;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent)
            });
    }
}
