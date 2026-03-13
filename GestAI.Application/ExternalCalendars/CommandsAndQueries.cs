using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GestAI.Application.ExternalCalendars;

public sealed record UpsertExternalCalendarConnectionCommand(
    int PropertyId,
    int? ConnectionId,
    int UnitId,
    ExternalChannelType ChannelType,
    string DisplayName,
    string ImportCalendarUrl,
    bool IsActive,
    bool AutoSyncEnabled) : IRequest<AppResult<int>>;

public sealed record DeleteExternalCalendarConnectionCommand(int PropertyId, int ConnectionId) : IRequest<AppResult>;
public sealed record SyncExternalCalendarConnectionCommand(int PropertyId, int ConnectionId) : IRequest<AppResult<ExternalSyncLogDto>>;
public sealed record GetExternalCalendarConnectionsQuery(int PropertyId, int? UnitId) : IRequest<AppResult<List<ExternalCalendarConnectionDto>>>;
public sealed record GetExternalCalendarEventsByRangeQuery(int PropertyId, DateOnly From, DateOnly To, int? UnitId = null) : IRequest<AppResult<List<ExternalCalendarEventDto>>>;
public sealed record GetExternalSyncLogsQuery(int PropertyId, int? ConnectionId = null) : IRequest<AppResult<List<ExternalSyncLogDto>>>;
public sealed record ExportUnitCalendarQuery(int UnitId, string Token) : IRequest<(bool Success, string Content, string FileName, string? ErrorMessage)>;

public sealed class UpsertExternalCalendarConnectionCommandValidator : AbstractValidator<UpsertExternalCalendarConnectionCommand>
{
    public UpsertExternalCalendarConnectionCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImportCalendarUrl).NotEmpty().Must(u => Uri.TryCreate(u, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)).WithMessage("La URL del calendario debe ser válida.");
    }
}

public sealed class ExternalCalendarHandler :
    IRequestHandler<UpsertExternalCalendarConnectionCommand, AppResult<int>>,
    IRequestHandler<DeleteExternalCalendarConnectionCommand, AppResult>,
    IRequestHandler<GetExternalCalendarConnectionsQuery, AppResult<List<ExternalCalendarConnectionDto>>>,
    IRequestHandler<GetExternalCalendarEventsByRangeQuery, AppResult<List<ExternalCalendarEventDto>>>,
    IRequestHandler<GetExternalSyncLogsQuery, AppResult<List<ExternalSyncLogDto>>>,
    IRequestHandler<SyncExternalCalendarConnectionCommand, AppResult<ExternalSyncLogDto>>,
    IRequestHandler<ExportUnitCalendarQuery, (bool Success, string Content, string FileName, string? ErrorMessage)>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IAuditService _audit;
    private readonly IExternalCalendarSyncService _syncService;
    private readonly IIcsCalendarService _icsService;
    private readonly IPropertyFeatureService _features;

    public ExternalCalendarHandler(IAppDbContext db, ICurrentUser current, IAuditService audit, IExternalCalendarSyncService syncService, IIcsCalendarService icsService, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _audit = audit;
        _syncService = syncService;
        _icsService = icsService;
        _features = features;
    }

    public async Task<AppResult<int>> Handle(UpsertExternalCalendarConnectionCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.ExternalCalendarSync, ct))
            return AppResult<int>.Fail("feature_disabled", "La sincronización externa está desactivada para este hospedaje.");

        var property = await _db.Properties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PropertyId && (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (property is null)
            return AppResult<int>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        var unit = await _db.Units.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UnitId && u.PropertyId == request.PropertyId, ct);
        if (unit is null)
            return AppResult<int>.Fail("not_found", "Unidad no encontrada.");
        if (!unit.IsActive)
            return AppResult<int>.Fail("unit_inactive", "No se puede vincular una unidad inactiva.");

        var normalizedUrl = request.ImportCalendarUrl.Trim();
        var duplicate = await _db.ExternalChannelConnections.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && x.UnitId == request.UnitId && x.ImportCalendarUrl == normalizedUrl)
            .Where(x => request.ConnectionId == null || x.Id != request.ConnectionId.Value)
            .AnyAsync(ct);
        if (duplicate)
            return AppResult<int>.Fail("duplicate", "Ya existe una conexión con esa URL para la unidad.");

        var isNew = request.ConnectionId is null;
        ExternalChannelConnection entity;
        if (isNew)
        {
            entity = new ExternalChannelConnection
            {
                PropertyId = request.PropertyId,
                UnitId = request.UnitId,
                ExportToken = Guid.NewGuid().ToString("N")
            };
            _db.ExternalChannelConnections.Add(entity);
        }
        else
        {
            entity = await _db.ExternalChannelConnections.FirstOrDefaultAsync(x => x.Id == request.ConnectionId!.Value && x.PropertyId == request.PropertyId, ct)
                ?? throw new InvalidOperationException("Conexión no encontrada.");
        }

        entity.ChannelType = request.ChannelType;
        entity.DisplayName = request.DisplayName.Trim();
        entity.ImportCalendarUrl = normalizedUrl;
        entity.IsActive = request.IsActive;
        entity.AutoSyncEnabled = request.AutoSyncEnabled;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(property.AccountId, request.PropertyId, "ExternalChannelConnection", entity.Id, isNew ? "Created" : "Updated", $"{entity.ChannelType}: {entity.DisplayName}", ct);

        return AppResult<int>.Ok(entity.Id);
    }

    public async Task<AppResult> Handle(DeleteExternalCalendarConnectionCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.ExternalCalendarSync, ct))
            return AppResult.Fail("feature_disabled", "La sincronización externa está desactivada para este hospedaje.");

        var entity = await _db.ExternalChannelConnections.Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == request.ConnectionId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (entity is null)
            return AppResult.Fail("not_found", "Conexión no encontrada.");

        entity.IsActive = false;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(entity.Property.AccountId, request.PropertyId, "ExternalChannelConnection", entity.Id, "Deactivated", $"{entity.ChannelType}: {entity.DisplayName}", ct);
        return AppResult.Ok();
    }

    public async Task<AppResult<List<ExternalCalendarConnectionDto>>> Handle(GetExternalCalendarConnectionsQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.ExternalCalendarSync, ct))
            return AppResult<List<ExternalCalendarConnectionDto>>.Fail("feature_disabled", "La sincronización externa está desactivada para este hospedaje.");

        var query = _db.ExternalChannelConnections.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)));
        if (request.UnitId.HasValue)
            query = query.Where(x => x.UnitId == request.UnitId.Value);

        var list = await query.OrderBy(x => x.Unit.Name).ThenBy(x => x.DisplayName)
            .Select(x => new ExternalCalendarConnectionDto(x.Id, x.PropertyId, x.UnitId, x.Unit.Name, x.ChannelType, x.DisplayName, x.ImportCalendarUrl, $"/api/public/unit-calendars/{x.UnitId}/ics?token={x.ExportToken}", x.IsActive, x.AutoSyncEnabled, x.LastSyncAt, x.LastSyncStatus, x.LastSyncMessage))
            .ToListAsync(ct);
        return AppResult<List<ExternalCalendarConnectionDto>>.Ok(list);
    }

    public async Task<AppResult<List<ExternalCalendarEventDto>>> Handle(GetExternalCalendarEventsByRangeQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.ExternalCalendarSync, ct))
            return AppResult<List<ExternalCalendarEventDto>>.Fail("feature_disabled", "La sincronización externa está desactivada para este hospedaje.");

        var query = _db.ExternalCalendarEvents.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && !x.IsCancelled)
            .Where(x => x.StartDate < request.To && request.From < x.EndDate)
            .Where(x => x.ExternalChannelConnection.IsActive)
            .Where(x => x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive));
        if (request.UnitId.HasValue)
            query = query.Where(x => x.UnitId == request.UnitId.Value);

        var list = await query.OrderBy(x => x.StartDate)
            .Select(x => new ExternalCalendarEventDto(x.Id, x.ExternalChannelConnectionId, x.UnitId, x.StartDate, x.EndDate, x.Summary, x.SourceChannel, x.IsCancelled))
            .ToListAsync(ct);
        return AppResult<List<ExternalCalendarEventDto>>.Ok(list);
    }

    public async Task<AppResult<List<ExternalSyncLogDto>>> Handle(GetExternalSyncLogsQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.ExternalCalendarSync, ct))
            return AppResult<List<ExternalSyncLogDto>>.Fail("feature_disabled", "La sincronización externa está desactivada para este hospedaje.");

        var query = _db.ExternalSyncLogs.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId)
            .Where(x => x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive));
        if (request.ConnectionId.HasValue)
            query = query.Where(x => x.ExternalChannelConnectionId == request.ConnectionId.Value);

        var list = await query.OrderByDescending(x => x.CreatedAtUtc).Take(50)
            .Select(x => new ExternalSyncLogDto(x.Id, x.ExternalChannelConnectionId, x.Status, x.ProcessedEvents, x.ImportedEvents, x.UpdatedEvents, x.CancelledEvents, x.Message, x.CreatedAtUtc))
            .ToListAsync(ct);
        return AppResult<List<ExternalSyncLogDto>>.Ok(list);
    }

    public async Task<AppResult<ExternalSyncLogDto>> Handle(SyncExternalCalendarConnectionCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.ExternalCalendarSync, ct))
            return AppResult<ExternalSyncLogDto>.Fail("feature_disabled", "La sincronización externa está desactivada para este hospedaje.");

        var connection = await _db.ExternalChannelConnections.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ConnectionId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (connection is null)
            return AppResult<ExternalSyncLogDto>.Fail("not_found", "Conexión no encontrada.");
        if (!connection.IsActive)
            return AppResult<ExternalSyncLogDto>.Fail("inactive", "La conexión está inactiva.");

        var result = await _syncService.SyncConnectionAsync(request.PropertyId, request.ConnectionId, _current.UserId, ct);
        var log = await _db.ExternalSyncLogs.AsNoTracking().Where(x => x.ExternalChannelConnectionId == request.ConnectionId).OrderByDescending(x => x.CreatedAtUtc).FirstAsync(ct);
        var dto = new ExternalSyncLogDto(log.Id, log.ExternalChannelConnectionId, log.Status, log.ProcessedEvents, log.ImportedEvents, log.UpdatedEvents, log.CancelledEvents, log.Message, log.CreatedAtUtc);
        return result.Success ? AppResult<ExternalSyncLogDto>.Ok(dto) : AppResult<ExternalSyncLogDto>.Fail("sync_error", result.Message);
    }

    public async Task<(bool Success, string Content, string FileName, string? ErrorMessage)> Handle(ExportUnitCalendarQuery request, CancellationToken ct)
    {
        var connection = await _db.ExternalChannelConnections.AsNoTracking().FirstOrDefaultAsync(x => x.UnitId == request.UnitId && x.ExportToken == request.Token && x.IsActive, ct);
        if (connection is null)
            return (false, string.Empty, string.Empty, "Calendario no encontrado.");

        var unit = await _db.Units.AsNoTracking().FirstAsync(x => x.Id == request.UnitId, ct);
        var property = await _db.Properties.AsNoTracking().FirstAsync(x => x.Id == unit.PropertyId, ct);
        var from = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-1));
        var to = from.AddYears(2);

        var icsEvents = new List<IcsCalendarEvent>();
        var bookings = await _db.Bookings.AsNoTracking()
            .Where(x => x.UnitId == unit.Id && x.Status != BookingStatus.Cancelled)
            .Where(x => x.CheckInDate < to && from < x.CheckOutDate)
            .ToListAsync(ct);
        icsEvents.AddRange(bookings.Select(x => new IcsCalendarEvent($"booking-{x.Id}@gestai", x.CheckInDate, x.CheckOutDate, $"Reserva interna {x.BookingCode}", false, null)));

        var blocks = await _db.BlockedDates.AsNoTracking()
            .Where(x => x.UnitId == unit.Id)
            .Where(x => x.DateFrom < to && from < x.DateTo)
            .ToListAsync(ct);
        icsEvents.AddRange(blocks.Select(x => new IcsCalendarEvent($"blocked-{x.Id}@gestai", x.DateFrom, x.DateTo, string.IsNullOrWhiteSpace(x.Reason) ? "Bloqueo manual" : x.Reason, false, null)));

        var content = _icsService.BuildUnitCalendar(property.Name, unit.Name, $"/api/public/unit-calendars/{unit.Id}/ics?token={request.Token}", icsEvents);
        var safeName = string.Concat(unit.Name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
        return (true, content, $"{safeName}.ics", null);
    }
}
