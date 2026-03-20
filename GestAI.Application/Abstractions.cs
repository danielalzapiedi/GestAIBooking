using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace GestAI.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<AccountUser> AccountUsers { get; }
    DbSet<AccountSubscriptionPlan> AccountSubscriptionPlans { get; }
    DbSet<SaasPlanDefinition> SaasPlanDefinitions { get; }
    DbSet<Property> Properties { get; }
    DbSet<PropertyFeatureSettings> PropertyFeatureSettings { get; }
    DbSet<Unit> Units { get; }
    DbSet<Guest> Guests { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingEvent> BookingEvents { get; }
    DbSet<Payment> Payments { get; }
    DbSet<BlockedDate> BlockedDates { get; }
    DbSet<RatePlan> RatePlans { get; }
    DbSet<SeasonalRate> SeasonalRates { get; }
    DbSet<DateRangeRate> DateRangeRates { get; }
    DbSet<MessageTemplate> MessageTemplates { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<SavedQuote> SavedQuotes { get; }
    DbSet<OperationalTask> OperationalTasks { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<ExternalChannelConnection> ExternalChannelConnections { get; }
    DbSet<ExternalCalendarEvent> ExternalCalendarEvents { get; }
    DbSet<ExternalSyncLog> ExternalSyncLogs { get; }
    DbSet<User> Users { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransactionAdapter> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);
    Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken ct = default);
}

public interface IDbContextTransactionAdapter : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

public interface IDateTime
{
    DateTime UtcNow { get; }
}

public interface ICurrentUser
{
    string UserId { get; }
    string? Email { get; }
    string? FullName { get; }
}

public interface IUserAccessService
{
    Task<int?> GetCurrentAccountIdAsync(CancellationToken ct);
    Task<int?> GetDefaultPropertyIdAsync(CancellationToken ct);
    Task<bool> HasPropertyAccessAsync(int propertyId, CancellationToken ct);
    Task<bool> HasPropertyModuleAccessAsync(int propertyId, GestAI.Domain.Enums.SaasModule module, CancellationToken ct);
    Task<GestAI.Domain.Entities.AccountUser?> GetMembershipAsync(int accountId, CancellationToken ct);
    Task<bool> HasModuleAccessAsync(int accountId, GestAI.Domain.Enums.SaasModule module, CancellationToken ct);
}

public interface ISaasPlanService
{
    Task<(bool Success, string? ErrorCode, string? Message)> ValidatePropertyCreationAsync(int accountId, CancellationToken ct);
    Task<(bool Success, string? ErrorCode, string? Message)> ValidateUnitCreationAsync(int accountId, CancellationToken ct);
    Task<(bool Success, string? ErrorCode, string? Message)> ValidateUserCreationAsync(int accountId, CancellationToken ct);
}

public interface IAuditService
{
    Task WriteAsync(int accountId, int? propertyId, string entityName, int? entityId, string action, string summary, CancellationToken ct);
}

public interface IIdentityService
{
    Task<(bool Success, string? UserId, string? Error)> CreateUserIfNotExistsAsync(string email, string password, CancellationToken ct);
    Task<(bool Success, string? UserId, string? Error)> CreateUserIfNotExistsAsync(string email, string password, CancellationToken ct, string firstName, string lastName, bool isActive, int? defaultPropertyId, int defaultAccountId);
    Task<(bool Success, string? UserId, string? Error)> FindUserIdByEmailAsync(string email, CancellationToken ct);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string newPassword, CancellationToken ct);
}

public interface IAccountResolver
{
    Task<int?> GetCurrentAccountIdAsync(string userId, CancellationToken ct);
    Task<bool> HasAccessAsync(string userId, int accountId, CancellationToken ct);
}

public interface IBookingAssistantService
{
    Task<string> GenerateGuestMessageAsync(BookingAssistantRequest request, CancellationToken ct);
}

public interface IQuoteSuggestionService
{
    Task<QuoteSuggestionResult> SuggestAsync(QuoteSuggestionRequest request, CancellationToken ct);
}

public interface IPropertyFeatureService
{
    Task<PropertyFeatureSettings> GetSettingsAsync(int propertyId, CancellationToken ct);
    Task<bool> IsEnabledAsync(int propertyId, GestAI.Domain.Enums.PropertyFeature feature, CancellationToken ct);
}

public sealed record BookingAssistantRequest(
    string GuestName,
    string PropertyName,
    string UnitName,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    decimal BalanceDue,
    string TemplateBody);

public sealed record QuoteSuggestionRequest(
    int PropertyId,
    int? UnitId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children);

public sealed record QuoteSuggestionResult(
    decimal SuggestedNightlyRate,
    decimal SuggestedTotal,
    string Summary);


public interface IExternalCalendarSyncService
{
    Task<ExternalSyncExecutionResult> SyncConnectionAsync(int propertyId, int connectionId, string? performedByUserId, CancellationToken ct);
}

public interface IIcsCalendarService
{
    Task<IReadOnlyList<IcsCalendarEvent>> LoadAsync(string url, CancellationToken ct);
    string BuildUnitCalendar(string propertyName, string unitName, string exportUrl, IReadOnlyCollection<IcsCalendarEvent> events);
}

public sealed record IcsCalendarEvent(
    string Uid,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Summary,
    bool IsCancelled,
    string? RawData);

public sealed record ExternalSyncExecutionResult(
    bool Success,
    string Message,
    int ProcessedEvents,
    int ImportedEvents,
    int UpdatedEvents,
    int CancelledEvents);
