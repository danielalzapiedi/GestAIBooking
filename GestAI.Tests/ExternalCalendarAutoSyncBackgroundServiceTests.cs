using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using GestAI.Infrastructure.Calendars;
using GestAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GestAI.Tests;

public class ExternalCalendarAutoSyncBackgroundServiceTests
{
    [Fact]
    public async Task Worker_Should_Not_Run_When_Disabled()
    {
        var recorder = new SyncRecorder();
        using var provider = BuildProvider(nameof(Worker_Should_Not_Run_When_Disabled), recorder, options: new ExternalCalendarAutoSyncOptions
        {
            Enabled = false,
            IntervalMinutes = 1,
            BatchSize = 20
        });

        await SeedConnectionsAsync(provider, [
            BuildConnection(1, 1, isActive: true, autoSync: true)
        ]);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var worker = CreateWorker(provider);
        await worker.StartAsync(cts.Token);
        await Task.Delay(150, CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        Assert.Empty(recorder.ConnectionIds);
    }

    [Fact]
    public async Task Worker_Should_Process_Only_Active_And_AutoSync_Connections()
    {
        var recorder = new SyncRecorder();
        using var provider = BuildProvider(nameof(Worker_Should_Process_Only_Active_And_AutoSync_Connections), recorder);

        await SeedConnectionsAsync(provider, [
            BuildConnection(1, 1, isActive: true, autoSync: true),
            BuildConnection(2, 1, isActive: true, autoSync: false),
            BuildConnection(3, 1, isActive: false, autoSync: true)
        ]);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var worker = CreateWorker(provider);
        await worker.StartAsync(cts.Token);
        await Task.Delay(150, CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        Assert.Single(recorder.ConnectionIds);
        Assert.Contains(1, recorder.ConnectionIds);
    }

    [Fact]
    public async Task Worker_Should_Continue_When_A_Connection_Fails()
    {
        var recorder = new SyncRecorder { ThrowOnConnectionId = 1 };
        using var provider = BuildProvider(nameof(Worker_Should_Continue_When_A_Connection_Fails), recorder);

        await SeedConnectionsAsync(provider, [
            BuildConnection(1, 1, isActive: true, autoSync: true),
            BuildConnection(2, 1, isActive: true, autoSync: true)
        ]);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        var worker = CreateWorker(provider);
        await worker.StartAsync(cts.Token);
        await Task.Delay(180, CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        Assert.Contains(1, recorder.ConnectionIds);
        Assert.Contains(2, recorder.ConnectionIds);
    }

    private static ExternalCalendarAutoSyncBackgroundService CreateWorker(ServiceProvider provider)
        => new(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<IOptionsMonitor<ExternalCalendarAutoSyncOptions>>(),
            NullLogger<ExternalCalendarAutoSyncBackgroundService>.Instance);

    private static ServiceProvider BuildProvider(string dbName, SyncRecorder recorder, ExternalCalendarAutoSyncOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton(recorder);
        services.AddScoped<IExternalCalendarSyncService, RecordingExternalCalendarSyncService>();
        services.AddSingleton<IOptionsMonitor<ExternalCalendarAutoSyncOptions>>(new StaticOptionsMonitor<ExternalCalendarAutoSyncOptions>(options ?? new ExternalCalendarAutoSyncOptions
        {
            Enabled = true,
            IntervalMinutes = 1,
            BatchSize = 20
        }));
        return services.BuildServiceProvider();
    }

    private static async Task SeedConnectionsAsync(ServiceProvider provider, IEnumerable<ExternalChannelConnection> connections)
    {
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.ExternalChannelConnections.AddRange(connections);
        await db.SaveChangesAsync();
    }

    private static ExternalChannelConnection BuildConnection(int id, int propertyId, bool isActive, bool autoSync)
        => new()
        {
            Id = id,
            PropertyId = propertyId,
            UnitId = 1,
            ChannelType = ExternalChannelType.Booking,
            DisplayName = $"Conn-{id}",
            ImportCalendarUrl = $"https://example.com/{id}.ics",
            ExportToken = Guid.NewGuid().ToString("N"),
            IsActive = isActive,
            AutoSyncEnabled = autoSync
        };

    private sealed class RecordingExternalCalendarSyncService(SyncRecorder recorder) : IExternalCalendarSyncService
    {
        public Task<ExternalSyncExecutionResult> SyncConnectionAsync(int propertyId, int connectionId, string? performedByUserId, CancellationToken ct)
        {
            recorder.ConnectionIds.Add(connectionId);
            if (recorder.ThrowOnConnectionId == connectionId)
                throw new InvalidOperationException("boom");

            return Task.FromResult(new ExternalSyncExecutionResult(true, "ok", 1, 1, 0, 0));
        }
    }

    private sealed class SyncRecorder
    {
        public List<int> ConnectionIds { get; } = new();
        public int? ThrowOnConnectionId { get; set; }
    }

    private sealed class StaticOptionsMonitor<T>(T currentValue) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = currentValue;
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
