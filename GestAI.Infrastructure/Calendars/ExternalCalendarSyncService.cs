using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GestAI.Infrastructure.Calendars;

public sealed class ExternalCalendarSyncService(IAppDbContext db, IIcsCalendarService icsService, IAuditService auditService) : IExternalCalendarSyncService
{
    private readonly IAppDbContext _db = db;
    private readonly IIcsCalendarService _icsService = icsService;
    private readonly IAuditService _audit = auditService;

    public async Task<ExternalSyncExecutionResult> SyncConnectionAsync(int propertyId, int connectionId, string? performedByUserId, CancellationToken ct)
    {
        var connection = await _db.ExternalChannelConnections.Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == connectionId && x.PropertyId == propertyId, ct)
            ?? throw new InvalidOperationException("Conexión no encontrada.");
        var processed = 0;
        var imported = 0;
        var updated = 0;
        var cancelled = 0;
        try
        {
            var feedEvents = await _icsService.LoadAsync(connection.ImportCalendarUrl, ct);
            var existing = await _db.ExternalCalendarEvents.Where(x => x.ExternalChannelConnectionId == connection.Id).ToListAsync(ct);
            var eventMap = existing.ToDictionary(x => x.ExternalEventUid, StringComparer.OrdinalIgnoreCase);
            var activeUids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var ev in feedEvents)
            {
                processed++;
                activeUids.Add(ev.Uid);
                var hash = ComputeHash(ev);
                if (!eventMap.TryGetValue(ev.Uid, out var current))
                {
                    current = new ExternalCalendarEvent
                    {
                        ExternalChannelConnectionId = connection.Id,
                        PropertyId = propertyId,
                        UnitId = connection.UnitId,
                        ExternalEventUid = ev.Uid,
                        SourceChannel = connection.ChannelType,
                    };
                    _db.ExternalCalendarEvents.Add(current);
                    imported++;
                }
                else if (current.SyncHash != hash || current.IsCancelled != ev.IsCancelled || current.StartDate != ev.StartDate || current.EndDate != ev.EndDate || current.Summary != ev.Summary)
                {
                    updated++;
                }

                current.StartDate = ev.StartDate;
                current.EndDate = ev.EndDate;
                current.Summary = ev.Summary;
                current.RawData = ev.RawData;
                current.SyncHash = hash;
                current.IsCancelled = ev.IsCancelled;
                current.ImportedAtUtc = DateTime.UtcNow;
                current.SourceChannel = connection.ChannelType;
            }

            foreach (var stale in existing.Where(x => !activeUids.Contains(x.ExternalEventUid) && !x.IsCancelled))
            {
                stale.IsCancelled = true;
                stale.ImportedAtUtc = DateTime.UtcNow;
                cancelled++;
            }

            connection.LastSyncAt = DateTime.UtcNow;
            connection.LastSyncStatus = ExternalSyncStatus.Success;
            connection.LastSyncMessage = $"Procesados {processed}. Nuevos {imported}, actualizados {updated}, cancelados {cancelled}.";
            connection.UpdatedAtUtc = DateTime.UtcNow;

            var log = new ExternalSyncLog
            {
                ExternalChannelConnectionId = connection.Id,
                PropertyId = propertyId,
                UnitId = connection.UnitId,
                Status = ExternalSyncStatus.Success,
                ProcessedEvents = processed,
                ImportedEvents = imported,
                UpdatedEvents = updated,
                CancelledEvents = cancelled,
                Message = connection.LastSyncMessage
            };
            _db.ExternalSyncLogs.Add(log);
            await _db.SaveChangesAsync(ct);
            await _audit.WriteAsync(connection.Property.AccountId, propertyId, "ExternalCalendarSync", connection.Id, "Sync", connection.LastSyncMessage ?? "Sync OK", ct);
            return new ExternalSyncExecutionResult(true, connection.LastSyncMessage ?? "Sync OK", processed, imported, updated, cancelled);
        }
        catch (Exception ex)
        {
            connection.LastSyncAt = DateTime.UtcNow;
            connection.LastSyncStatus = ExternalSyncStatus.Error;
            connection.LastSyncMessage = ex.Message.Length > 1800 ? ex.Message[..1800] : ex.Message;
            connection.UpdatedAtUtc = DateTime.UtcNow;
            _db.ExternalSyncLogs.Add(new ExternalSyncLog
            {
                ExternalChannelConnectionId = connection.Id,
                PropertyId = propertyId,
                UnitId = connection.UnitId,
                Status = ExternalSyncStatus.Error,
                ProcessedEvents = processed,
                ImportedEvents = imported,
                UpdatedEvents = updated,
                CancelledEvents = cancelled,
                Message = connection.LastSyncMessage
            });
            await _db.SaveChangesAsync(ct);
            await _audit.WriteAsync(connection.Property.AccountId, propertyId, "ExternalCalendarSync", connection.Id, "SyncError", connection.LastSyncMessage ?? "Sync error", ct);
            return new ExternalSyncExecutionResult(false, connection.LastSyncMessage ?? "Error de sincronización.", processed, imported, updated, cancelled);
        }
    }

    private static string ComputeHash(IcsCalendarEvent ev)
    {
        var payload = $"{ev.Uid}|{ev.StartDate:yyyyMMdd}|{ev.EndDate:yyyyMMdd}|{ev.Summary}|{ev.IsCancelled}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
