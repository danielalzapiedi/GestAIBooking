using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class Property : Entity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? CommercialName { get; set; }
    public int Type { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public TimeOnly? DefaultCheckInTime { get; set; }
    public TimeOnly? DefaultCheckOutTime { get; set; }
    public string Currency { get; set; } = "ARS";
    public string? DepositPolicy { get; set; }
    public decimal DefaultDepositPercentage { get; set; } = 0m;
    public string? CancellationPolicy { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? CheckInInstructions { get; set; }
    public string? PropertyRules { get; set; }
    public string? CommercialContactName { get; set; }
    public string? CommercialContactPhone { get; set; }
    public string? CommercialContactEmail { get; set; }
    public string? PublicSlug { get; set; }
    public string? PublicDescription { get; set; }
    public PropertyFeatureSettings FeatureSettings { get; set; } = null!;
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
    public ICollection<MessageTemplate> MessageTemplates { get; set; } = new List<MessageTemplate>();
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    public ICollection<OperationalTask> Tasks { get; set; } = new List<OperationalTask>();
}
