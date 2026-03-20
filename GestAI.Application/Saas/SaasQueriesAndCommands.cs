using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Saas;

public sealed class GetCurrentUserAccessQueryHandler : IRequestHandler<GetCurrentUserAccessQuery, AppResult<CurrentUserAccessDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;

    public GetCurrentUserAccessQueryHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access)
    {
        _db = db;
        _current = current;
        _access = access;
    }

    public async Task<AppResult<CurrentUserAccessDto>> Handle(GetCurrentUserAccessQuery request, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().FirstAsync(x => x.Id == _current.UserId, ct);
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        InternalUserRole? role = null;
        var isOwner = false;
        SaasPlanDefinition? plan = null;

        if (accountId.HasValue)
        {
            var accountData = await _db.Accounts.AsNoTracking()
                .Where(x => x.Id == accountId.Value)
                .Select(x => new
                {
                    IsOwner = x.OwnerUserId == _current.UserId,
                    Role = x.Users.Where(u => u.UserId == _current.UserId && u.IsActive).Select(u => (InternalUserRole?)u.Role).FirstOrDefault(),
                    Plan = x.SubscriptionPlans.Where(p => p.IsActive).OrderByDescending(p => p.StartedAtUtc)
                        .Select(p => new SaasPlanDefinition
                        {
                            Id = p.PlanDefinition.Id,
                            Code = p.PlanDefinition.Code,
                            Name = p.PlanDefinition.Name,
                            MaxProperties = p.PlanDefinition.MaxProperties,
                            MaxUnits = p.PlanDefinition.MaxUnits,
                            MaxUsers = p.PlanDefinition.MaxUsers,
                            IncludesReports = p.PlanDefinition.IncludesReports,
                            IncludesPublicPortal = p.PlanDefinition.IncludesPublicPortal,
                            IncludesOperations = p.PlanDefinition.IncludesOperations
                        }).FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);

            if (accountData is not null)
            {
                isOwner = accountData.IsOwner;
                role = isOwner ? InternalUserRole.Owner : accountData.Role;
                plan = accountData.Plan;
            }
        }

        var modules = Enum.GetValues<SaasModule>()
            .Select(module => new ModuleAccessDto(module, SaasPermissionMap.HasAccess(role, plan, module, isOwner)))
            .ToList();

        return AppResult<CurrentUserAccessDto>.Ok(new CurrentUserAccessDto(accountId, user.DefaultPropertyId, role, isOwner, user.IsActive, modules));
    }
}

public sealed class GetAccountPlanOptionsQueryHandler : IRequestHandler<GetAccountPlanOptionsQuery, AppResult<List<AccountPlanOptionDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;

    public GetAccountPlanOptionsQueryHandler(IAppDbContext db, IUserAccessService access)
    {
        _db = db;
        _access = access;
    }

    public async Task<AppResult<List<AccountPlanOptionDto>>> Handle(GetAccountPlanOptionsQuery request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult<List<AccountPlanOptionDto>>.Fail("account_required", "No se encontró una cuenta activa.");
        if (!await _access.HasModuleAccessAsync(accountId.Value, SaasModule.Configuration, ct))
            return AppResult<List<AccountPlanOptionDto>>.Fail("forbidden", "No tenés permisos para administrar la cuenta.");

        var plans = await _db.SaasPlanDefinitions.AsNoTracking()
            .OrderBy(x => x.MaxProperties)
            .ThenBy(x => x.MaxUnits)
            .ThenBy(x => x.MaxUsers)
            .Select(x => new AccountPlanOptionDto(
                x.Id,
                x.Code,
                x.Name,
                x.MaxProperties,
                x.MaxUnits,
                x.MaxUsers,
                x.IncludesReports,
                x.IncludesOperations,
                x.IncludesPublicPortal))
            .ToListAsync(ct);

        return AppResult<List<AccountPlanOptionDto>>.Ok(plans);
    }
}

public sealed class GetAccountSummaryQueryHandler : IRequestHandler<GetAccountSummaryQuery, AppResult<AccountSummaryDto>>
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;

    public GetAccountSummaryQueryHandler(IAppDbContext db, IUserAccessService access)
    {
        _db = db;
        _access = access;
    }

    public async Task<AppResult<AccountSummaryDto>> Handle(GetAccountSummaryQuery request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult<AccountSummaryDto>.Fail("account_required", "No se encontró una cuenta activa.");

        var account = await _db.Accounts.AsNoTracking()
            .Where(x => x.Id == accountId.Value)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.IsActive,
                x.CreatedAtUtc,
                Plan = x.SubscriptionPlans.Where(p => p.IsActive).OrderByDescending(p => p.StartedAtUtc)
                    .Select(p => new
                    {
                        p.PlanDefinitionId,
                        p.StartedAtUtc,
                        p.IsActive,
                        p.PlanDefinition.Name,
                        p.PlanDefinition.Code,
                        p.PlanDefinition.MaxProperties,
                        p.PlanDefinition.MaxUnits,
                        p.PlanDefinition.MaxUsers,
                        p.PlanDefinition.IncludesReports,
                        p.PlanDefinition.IncludesOperations
                    }).FirstOrDefault(),
                Properties = x.Properties.OrderBy(p => p.Name).Select(p => new PropertyUsageItemDto(p.Id, p.Name, p.IsActive, p.Units.Count())).ToList(),
                UsersCount = x.Users.Count(),
                UnitsCount = x.Properties.SelectMany(p => p.Units).Count()
            })
            .FirstOrDefaultAsync(ct);

        if (account is null || account.Plan is null) return AppResult<AccountSummaryDto>.Fail("account_required", "No se encontró el resumen de cuenta.");

        var dto = new AccountSummaryDto(
            account.Id,
            account.Name,
            account.IsActive,
            account.CreatedAtUtc,
            account.Plan.Name,
            account.Plan.Code,
            account.Plan.IsActive ? "Activo" : "Inactivo",
            account.Plan.StartedAtUtc,
            account.Plan.MaxProperties,
            account.Plan.MaxUnits,
            account.Plan.MaxUsers,
            account.Plan.IncludesReports,
            account.Plan.IncludesOperations,
            account.Properties.Count,
            account.UnitsCount,
            account.UsersCount,
            account.Properties);

        return AppResult<AccountSummaryDto>.Ok(dto);
    }
}

public sealed class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;
    private readonly IAuditService _audit;

    public UpdateAccountCommandHandler(IAppDbContext db, IUserAccessService access, IAuditService audit)
    {
        _db = db;
        _access = access;
        _audit = audit;
    }

    public async Task<AppResult> Handle(UpdateAccountCommand request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult.Fail("account_required", "No se encontró una cuenta activa.");
        if (!await _access.HasModuleAccessAsync(accountId.Value, SaasModule.Configuration, ct))
            return AppResult.Fail("forbidden", "No tenés permisos para administrar la cuenta.");

        var account = await _db.Accounts.FirstAsync(x => x.Id == accountId.Value, ct);
        account.Name = request.Name.Trim();
        var auditMessages = new List<string> { $"Cuenta actualizada: {account.Name}" };

        if (request.PlanDefinitionId.HasValue)
        {
            var currentPlan = await _db.AccountSubscriptionPlans.FirstOrDefaultAsync(x => x.AccountId == accountId.Value && x.IsActive, ct);
            if (currentPlan is null) return AppResult.Fail("plan_required", "La cuenta no tiene plan activo.");
            if (currentPlan.PlanDefinitionId != request.PlanDefinitionId.Value)
            {
                currentPlan.IsActive = false;
                currentPlan.ChangedAtUtc = DateTime.UtcNow;
                _db.AccountSubscriptionPlans.Add(new AccountSubscriptionPlan
                {
                    AccountId = accountId.Value,
                    PlanDefinitionId = request.PlanDefinitionId.Value,
                    IsActive = true,
                    StartedAtUtc = DateTime.UtcNow,
                    ChangedAtUtc = DateTime.UtcNow
                });
                var planName = await _db.SaasPlanDefinitions.Where(x => x.Id == request.PlanDefinitionId.Value).Select(x => x.Name).FirstOrDefaultAsync(ct);
                if (string.IsNullOrWhiteSpace(planName))
                    return AppResult.Fail("plan_not_found", "El plan seleccionado no existe.");

                auditMessages.Add($"Plan cambiado a {planName}");
            }
        }

        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(accountId.Value, null, "Account", account.Id, "updated", string.Join(". ", auditMessages), ct);
        return AppResult.Ok();
    }
}

public sealed class GetAccountUsersQueryHandler : IRequestHandler<GetAccountUsersQuery, AppResult<List<AccountUserListItemDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;

    public GetAccountUsersQueryHandler(IAppDbContext db, IUserAccessService access)
    {
        _db = db;
        _access = access;
    }

    public async Task<AppResult<List<AccountUserListItemDto>>> Handle(GetAccountUsersQuery request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult<List<AccountUserListItemDto>>.Fail("account_required", "No se encontró una cuenta activa.");
        if (!await _access.HasModuleAccessAsync(accountId.Value, SaasModule.Users, ct))
            return AppResult<List<AccountUserListItemDto>>.Fail("forbidden", "No tenés permisos para ver usuarios.");

        var rawItems = await _db.AccountUsers.AsNoTracking()
            .Where(x => x.AccountId == accountId.Value)
            .OrderBy(x => x.Role).ThenBy(x => x.User.Nombre).ThenBy(x => x.User.Apellido)
            .Select(x => new
            {
                x.UserId,
                x.User.Nombre,
                x.User.Apellido,
                Email = x.User.Email ?? string.Empty,
                IsActive = x.IsActive && x.User.IsActive,
                x.Role,
                x.User.DefaultPropertyId,
                x.User.LastLoginAtUtc,
                x.InvitedAtUtc
            })
            .ToListAsync(ct);

        var propertyNames = await _db.Properties.AsNoTracking()
            .Where(x => x.AccountId == accountId.Value)
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var items = rawItems.Select(x => new AccountUserListItemDto(
            x.UserId,
            (x.Nombre + " " + x.Apellido).Trim(),
            x.Email,
            x.IsActive,
            x.Role,
            x.DefaultPropertyId,
            x.DefaultPropertyId.HasValue && propertyNames.TryGetValue(x.DefaultPropertyId.Value, out var propertyName) ? propertyName : null,
            x.LastLoginAtUtc,
            x.InvitedAtUtc)).ToList();

        return AppResult<List<AccountUserListItemDto>>.Ok(items);
    }
}

public sealed class UpsertAccountUserCommandValidator : AbstractValidator<UpsertAccountUserCommand>
{
    public UpsertAccountUserCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).When(x => string.IsNullOrWhiteSpace(x.UserId));
        RuleFor(x => x.Password).MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.Password));
    }
}

public sealed class UpsertAccountUserCommandHandler : IRequestHandler<UpsertAccountUserCommand, AppResult<string>>
{
    private readonly IAppDbContext _db;
    private readonly IIdentityService _identity;
    private readonly IUserAccessService _access;
    private readonly ISaasPlanService _plan;
    private readonly IAuditService _audit;

    public UpsertAccountUserCommandHandler(IAppDbContext db, IIdentityService identity, IUserAccessService access, ISaasPlanService plan, IAuditService audit)
    {
        _db = db;
        _identity = identity;
        _access = access;
        _plan = plan;
        _audit = audit;
    }

    public async Task<AppResult<string>> Handle(UpsertAccountUserCommand request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult<string>.Fail("account_required", "No se encontró una cuenta activa.");
        if (!await _access.HasModuleAccessAsync(accountId.Value, SaasModule.Users, ct))
            return AppResult<string>.Fail("forbidden", "No tenés permisos para administrar usuarios.");

        if (request.DefaultPropertyId.HasValue)
        {
            var propertyValid = await _db.Properties.AsNoTracking().AnyAsync(x => x.Id == request.DefaultPropertyId.Value && x.AccountId == accountId.Value, ct);
            if (!propertyValid) return AppResult<string>.Fail("invalid_property", "El hospedaje por defecto no pertenece a la cuenta.");
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            var limit = await _plan.ValidateUserCreationAsync(accountId.Value, ct);
            if (!limit.Success) return AppResult<string>.Fail(limit.ErrorCode!, limit.Message!);

            var existing = await _identity.FindUserIdByEmailAsync(request.Email.Trim(), ct);
            if (!string.IsNullOrWhiteSpace(existing.UserId))
                return AppResult<string>.Fail("email_exists", "Ya existe un usuario con ese email.");

            var create = await _identity.CreateUserIfNotExistsAsync(request.Email.Trim(), request.Password!, ct, request.Nombre.Trim(), request.Apellido.Trim(), request.IsActive, request.DefaultPropertyId, accountId.Value);
            if (!create.Success || string.IsNullOrWhiteSpace(create.UserId)) return AppResult<string>.Fail("create_user_failed", create.Error ?? "No se pudo crear el usuario.");

            _db.AccountUsers.Add(new AccountUser
            {
                AccountId = accountId.Value,
                UserId = create.UserId,
                Role = request.Role,
                IsActive = request.IsActive,
                CanManageBookings = true,
                CanManageGuests = true,
                CanManagePayments = request.Role is InternalUserRole.Admin or InternalUserRole.Owner or InternalUserRole.Reception,
                CanViewReports = request.Role is InternalUserRole.Admin or InternalUserRole.Owner,
                CanManageConfiguration = request.Role is InternalUserRole.Admin or InternalUserRole.Owner,
                InvitedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            await _audit.WriteAsync(accountId.Value, request.DefaultPropertyId, "AccountUser", null, "created", $"Usuario creado: {request.Nombre} {request.Apellido} ({request.Email}) - rol {request.Role}", ct);
            return AppResult<string>.Ok(create.UserId);
        }

        var membership = await _db.AccountUsers.Include(x => x.User).FirstOrDefaultAsync(x => x.AccountId == accountId.Value && x.UserId == request.UserId, ct);
        if (membership is null) return AppResult<string>.Fail("not_found", "Usuario no encontrado.");

        membership.Role = request.Role;
        membership.IsActive = request.IsActive;
        membership.CanManagePayments = request.Role is InternalUserRole.Admin or InternalUserRole.Owner or InternalUserRole.Reception;
        membership.CanViewReports = request.Role is InternalUserRole.Admin or InternalUserRole.Owner;
        membership.CanManageConfiguration = request.Role is InternalUserRole.Admin or InternalUserRole.Owner;
        membership.User.Nombre = request.Nombre.Trim();
        membership.User.Apellido = request.Apellido.Trim();
        membership.User.Email = request.Email.Trim();
        membership.User.UserName = request.Email.Trim();
        membership.User.NormalizedEmail = request.Email.Trim().ToUpperInvariant();
        membership.User.NormalizedUserName = request.Email.Trim().ToUpperInvariant();
        membership.User.IsActive = request.IsActive;
        membership.User.DefaultPropertyId = request.DefaultPropertyId;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordReset = await _identity.ResetPasswordAsync(membership.UserId, request.Password.Trim(), ct);
            if (!passwordReset.Success)
                return AppResult<string>.Fail("password_reset_failed", passwordReset.Error ?? "No se pudo actualizar la contraseña del usuario.");
        }

        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(accountId.Value, request.DefaultPropertyId, "AccountUser", null, "updated", $"Usuario actualizado: {request.Nombre} {request.Apellido} ({request.Email}) - rol {request.Role} - activo: {(request.IsActive ? "sí" : "no")}{(string.IsNullOrWhiteSpace(request.Password) ? string.Empty : " - contraseña regenerada")}", ct);
        return AppResult<string>.Ok(membership.UserId);
    }
}

public sealed class ToggleAccountUserStatusCommandHandler : IRequestHandler<ToggleAccountUserStatusCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;
    private readonly IAuditService _audit;

    public ToggleAccountUserStatusCommandHandler(IAppDbContext db, IUserAccessService access, IAuditService audit)
    {
        _db = db;
        _access = access;
        _audit = audit;
    }

    public async Task<AppResult> Handle(ToggleAccountUserStatusCommand request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult.Fail("account_required", "No se encontró una cuenta activa.");
        if (!await _access.HasModuleAccessAsync(accountId.Value, SaasModule.Users, ct))
            return AppResult.Fail("forbidden", "No tenés permisos para administrar usuarios.");

        var membership = await _db.AccountUsers.Include(x => x.User).FirstOrDefaultAsync(x => x.AccountId == accountId.Value && x.UserId == request.UserId, ct);
        if (membership is null) return AppResult.Fail("not_found", "Usuario no encontrado.");

        membership.IsActive = request.IsActive;
        membership.User.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(accountId.Value, membership.User.DefaultPropertyId, "AccountUser", null, request.IsActive ? "activated" : "deactivated", $"Usuario {(request.IsActive ? "activado" : "desactivado")}: {membership.User.Nombre} {membership.User.Apellido}", ct);
        return AppResult.Ok();
    }
}

public sealed class GetAccountAuditQueryHandler : IRequestHandler<GetAccountAuditQuery, AppResult<List<AccountAuditItemDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;

    public GetAccountAuditQueryHandler(IAppDbContext db, IUserAccessService access)
    {
        _db = db;
        _access = access;
    }

    public async Task<AppResult<List<AccountAuditItemDto>>> Handle(GetAccountAuditQuery request, CancellationToken ct)
    {
        var accountId = await _access.GetCurrentAccountIdAsync(ct);
        if (!accountId.HasValue) return AppResult<List<AccountAuditItemDto>>.Fail("account_required", "No se encontró una cuenta activa.");

        var query = _db.AuditLogs.AsNoTracking()
            .Where(x => x.AccountId == accountId.Value);

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(x => x.EntityName == request.EntityName);

        if (!string.IsNullOrWhiteSpace(request.UserName))
            query = query.Where(x => x.UserName != null && x.UserName.Contains(request.UserName));

        if (request.From.HasValue)
        {
            var fromUtc = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAtUtc >= fromUtc);
        }

        if (request.To.HasValue)
        {
            var toUtcExclusive = request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAtUtc < toUtcExclusive);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(request.Take)
            .Select(x => new AccountAuditItemDto(x.CreatedAtUtc, x.EntityName, x.Action, x.Summary ?? string.Empty, x.UserName))
            .ToListAsync(ct);
        return AppResult<List<AccountAuditItemDto>>.Ok(items);
    }
}
