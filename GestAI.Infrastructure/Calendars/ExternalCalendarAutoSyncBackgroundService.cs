using GestAI.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GestAI.Infrastructure.Calendars;

public sealed class ExternalCalendarAutoSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<ExternalCalendarAutoSyncOptions> optionsMonitor,
    ILogger<ExternalCalendarAutoSyncBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IOptionsMonitor<ExternalCalendarAutoSyncOptions> _optionsMonitor = optionsMonitor;
    private readonly ILogger<ExternalCalendarAutoSyncBackgroundService> _logger = logger;
    private static readonly ConcurrentDictionary<int, byte> _connectionLocks = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("External calendar auto-sync worker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var cycleTimer = Stopwatch.StartNew();
            var options = _optionsMonitor.CurrentValue;
            var intervalMinutes = Math.Max(1, options.IntervalMinutes);

            try
            {
                if (!options.Enabled)
                {
                    _logger.LogInformation("External calendar auto-sync deshabilitado por configuración global.");
                }
                else
                {
                    await RunSyncCycleAsync(options, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en ciclo de auto-sync de calendarios externos.");
            }
            finally
            {
                cycleTimer.Stop();
                _logger.LogInformation("Ciclo de auto-sync finalizado en {ElapsedMs} ms.", cycleTimer.ElapsedMilliseconds);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("External calendar auto-sync worker detenido.");
    }

    private async Task RunSyncCycleAsync(ExternalCalendarAutoSyncOptions options, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var batchSize = Math.Max(1, options.BatchSize);
        var connections = await db.ExternalChannelConnections
            .AsNoTracking()
            .Where(x => x.IsActive && x.AutoSyncEnabled)
            .OrderBy(x => x.LastSyncAt ?? DateTime.MinValue)
            .Take(batchSize)
            .Select(x => new { x.Id, x.PropertyId })
            .ToListAsync(ct);

        _logger.LogInformation("Auto-sync encontró {Count} conexiones activas con auto-sync habilitado.", connections.Count);

        foreach (var connection in connections)
        {
            if (!_connectionLocks.TryAdd(connection.Id, 0))
            {
                _logger.LogDebug("Se omite conexión {ConnectionId} por sync en ejecución.", connection.Id);
                continue;
            }

            try
            {
                await SyncConnectionAsync(connection.PropertyId, connection.Id, ct);
            }
            finally
            {
                _connectionLocks.TryRemove(connection.Id, out _);
            }
        }
    }

    private async Task SyncConnectionAsync(int propertyId, int connectionId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var syncService = scope.ServiceProvider.GetRequiredService<IExternalCalendarSyncService>();

        try
        {
            var result = await syncService.SyncConnectionAsync(propertyId, connectionId, performedByUserId: null, ct);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Auto-sync OK conexión {ConnectionId}. Procesados {Processed}. Nuevos {Imported}, actualizados {Updated}, cancelados {Cancelled}.",
                    connectionId,
                    result.ProcessedEvents,
                    result.ImportedEvents,
                    result.UpdatedEvents,
                    result.CancelledEvents);
            }
            else
            {
                _logger.LogWarning("Auto-sync con error de negocio en conexión {ConnectionId}: {Message}", connectionId, result.Message);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-sync falló para conexión {ConnectionId}.", connectionId);
        }
    }
}
