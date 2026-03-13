using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Properties;

public sealed record UpsertPropertyCommand(
    int? PropertyId,
    string Name,
    string? CommercialName,
    int Type,
    bool IsActive,
    string? Phone,
    string? Email,
    string? City,
    string? Province,
    string? Country,
    string? Address,
    TimeOnly? DefaultCheckInTime,
    TimeOnly? DefaultCheckOutTime,
    string Currency,
    string? DepositPolicy,
    decimal DefaultDepositPercentage,
    string? CancellationPolicy,
    string? TermsAndConditions,
    string? CheckInInstructions,
    string? PropertyRules,
    string? CommercialContactName,
    string? CommercialContactPhone,
    string? CommercialContactEmail,
    string? PublicSlug,
    string? PublicDescription) : IRequest<AppResult<int>>;

public sealed class UpsertPropertyCommandValidator : AbstractValidator<UpsertPropertyCommand>
{
    public UpsertPropertyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);
        RuleFor(x => x.DefaultDepositPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.CommercialContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.CommercialContactEmail));
        RuleFor(x => x.PublicSlug).Matches("^[a-z0-9-]+$").When(x => !string.IsNullOrWhiteSpace(x.PublicSlug));
    }
}

public sealed class UpsertPropertyCommandHandler : IRequestHandler<UpsertPropertyCommand, AppResult<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly ISaasPlanService _plan;
    private readonly IAuditService _audit;

    public UpsertPropertyCommandHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access, ISaasPlanService plan, IAuditService audit)
    {
        _db = db;
        _current = current;
        _access = access;
        _plan = plan;
        _audit = audit;
    }

    public async Task<AppResult<int>> Handle(UpsertPropertyCommand request, CancellationToken ct)
    {
        var normalizedSlug = string.IsNullOrWhiteSpace(request.PublicSlug)
            ? null
            : request.PublicSlug.Trim().ToLowerInvariant();

        if (normalizedSlug is not null)
        {
            var slugExists = await _db.Properties.AsNoTracking()
                .AnyAsync(x => x.PublicSlug == normalizedSlug && x.Id != (request.PropertyId ?? 0), ct);
            if (slugExists)
                return AppResult<int>.Fail("slug_exists", "El slug público ya está en uso.");
        }

        Property property;
        int accountId;
        if (request.PropertyId is null or <= 0)
        {
            accountId = await _access.GetCurrentAccountIdAsync(ct) ?? 0;
            if (accountId <= 0)
                return AppResult<int>.Fail("account_required", "Necesitás una cuenta activa para crear un hospedaje.");
            if (!await _access.HasModuleAccessAsync(accountId, SaasModule.Properties, ct))
                return AppResult<int>.Fail("forbidden", "No tenés permisos para crear hospedajes.");

            var limit = await _plan.ValidatePropertyCreationAsync(accountId, ct);
            if (!limit.Success) return AppResult<int>.Fail(limit.ErrorCode!, limit.Message!);

            property = new Property
            {
                AccountId = accountId,
                IsActive = request.IsActive
            };
            _db.Properties.Add(property);
            _db.PropertyFeatureSettings.Add(new PropertyFeatureSettings
            {
                Property = property
            });
        }
        else
        {
            property = await _db.Properties
                .FirstOrDefaultAsync(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct)
                ?? throw new InvalidOperationException("Hospedaje no encontrado.");
            accountId = property.AccountId;
        }

        property.Name = request.Name.Trim();
        property.CommercialName = request.CommercialName?.Trim();
        property.Type = request.Type;
        property.IsActive = request.IsActive;
        property.Phone = request.Phone?.Trim();
        property.Email = request.Email?.Trim();
        property.City = request.City?.Trim();
        property.Province = request.Province?.Trim();
        property.Country = request.Country?.Trim();
        property.Address = request.Address?.Trim();
        property.DefaultCheckInTime = request.DefaultCheckInTime;
        property.DefaultCheckOutTime = request.DefaultCheckOutTime;
        property.Currency = request.Currency.Trim().ToUpperInvariant();
        property.DepositPolicy = request.DepositPolicy?.Trim();
        property.DefaultDepositPercentage = request.DefaultDepositPercentage;
        property.CancellationPolicy = request.CancellationPolicy?.Trim();
        property.TermsAndConditions = request.TermsAndConditions?.Trim();
        property.CheckInInstructions = request.CheckInInstructions?.Trim();
        property.PropertyRules = request.PropertyRules?.Trim();
        property.CommercialContactName = request.CommercialContactName?.Trim();
        property.CommercialContactPhone = request.CommercialContactPhone?.Trim();
        property.CommercialContactEmail = request.CommercialContactEmail?.Trim();
        property.PublicSlug = normalizedSlug;
        property.PublicDescription = request.PublicDescription?.Trim();

        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(accountId, property.Id, "Property", property.Id, request.PropertyId is null ? "created" : "updated", $"Hospedaje {(request.PropertyId is null ? "creado" : "actualizado")}: {property.Name}", ct);
        return AppResult<int>.Ok(property.Id);
    }
}
