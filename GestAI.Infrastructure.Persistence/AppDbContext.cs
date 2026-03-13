using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GestAI.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountUser> AccountUsers => Set<AccountUser>();
    public DbSet<AccountSubscriptionPlan> AccountSubscriptionPlans => Set<AccountSubscriptionPlan>();
    public DbSet<SaasPlanDefinition> SaasPlanDefinitions => Set<SaasPlanDefinition>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyFeatureSettings> PropertyFeatureSettings => Set<PropertyFeatureSettings>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingEvent> BookingEvents => Set<BookingEvent>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BlockedDate> BlockedDates => Set<BlockedDate>();
    public DbSet<RatePlan> RatePlans => Set<RatePlan>();
    public DbSet<SeasonalRate> SeasonalRates => Set<SeasonalRate>();
    public DbSet<DateRangeRate> DateRangeRates => Set<DateRangeRate>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<SavedQuote> SavedQuotes => Set<SavedQuote>();
    public DbSet<OperationalTask> OperationalTasks => Set<OperationalTask>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ExternalChannelConnection> ExternalChannelConnections => Set<ExternalChannelConnection>();
    public DbSet<ExternalCalendarEvent> ExternalCalendarEvents => Set<ExternalCalendarEvent>();
    public DbSet<ExternalSyncLog> ExternalSyncLogs => Set<ExternalSyncLog>();
    DbSet<User> IAppDbContext.Users => Set<User>();

    public async Task<IDbContextTransactionAdapter> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default)
    {
        var tx = await Database.BeginTransactionAsync(isolationLevel, ct);
        return new EfTxAdapter(tx);
    }

    public Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken ct = default)
        => Database.ExecuteSqlInterpolatedAsync(sql, ct);

    private sealed class EfTxAdapter : IDbContextTransactionAdapter
    {
        private readonly Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction _tx;
        public EfTxAdapter(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx) => _tx = tx;
        public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);
        public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);
        public ValueTask DisposeAsync() => _tx.DisposeAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
