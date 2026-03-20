namespace GestAI.Web.Dtos;

public enum InternalUserRole
{
    Owner = 0,
    Admin = 1,
    Reception = 2,
    Operations = 3
}

public enum SaasPlanCode
{
    Starter = 0,
    Pro = 1,
    Manager = 2
}

public enum SaasModule
{
    Dashboard = 0,
    Properties = 1,
    Units = 2,
    Rates = 3,
    Promotions = 4,
    Bookings = 5,
    Guests = 6,
    Payments = 7,
    Housekeeping = 8,
    Reports = 9,
    Users = 10,
    Configuration = 11
}

public sealed record ModuleAccessDto(SaasModule Module, bool Allowed);
public sealed record CurrentUserAccessDto(int? AccountId, int? DefaultPropertyId, InternalUserRole? Role, bool IsOwner, bool IsActive, List<ModuleAccessDto> Modules);
public sealed record PropertyUsageItemDto(int Id, string Name, bool IsActive, int UnitsCount);
public sealed record AccountSummaryDto(int AccountId, string Name, bool IsActive, DateTime CreatedAtUtc, string PlanName, SaasPlanCode PlanCode, string PlanStatus, DateTime StartedAtUtc, int MaxProperties, int MaxUnits, int MaxUsers, bool IncludesReports, bool IncludesOperations, int CurrentProperties, int CurrentUnits, int CurrentUsers, List<PropertyUsageItemDto> Properties);
public sealed record AccountPlanOptionDto(int Id, SaasPlanCode Code, string Name, int MaxProperties, int MaxUnits, int MaxUsers, bool IncludesReports, bool IncludesOperations, bool IncludesPublicPortal);
public sealed record UpdateAccountCommand(string Name, int? PlanDefinitionId);
public sealed record AccountUserListItemDto(string UserId, string DisplayName, string Email, bool IsActive, InternalUserRole Role, int? DefaultPropertyId, string? DefaultPropertyName, DateTime? LastLoginAtUtc, DateTime InvitedAtUtc);
public sealed class UpsertAccountUserCommand
{
    public string? UserId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public InternalUserRole Role { get; set; }
    public int? DefaultPropertyId { get; set; }
    public string? Password { get; set; }

    public UpsertAccountUserCommand() { }

    public UpsertAccountUserCommand(string? userId, string nombre, string apellido, string email, bool isActive, InternalUserRole role, int? defaultPropertyId, string? password)
    {
        UserId = userId;
        Nombre = nombre;
        Apellido = apellido;
        Email = email;
        IsActive = isActive;
        Role = role;
        DefaultPropertyId = defaultPropertyId;
        Password = password;
    }
}

public sealed record AccountAuditItemDto(DateTime CreatedAtUtc, string EntityName, string Action, string Summary, string? UserName);
