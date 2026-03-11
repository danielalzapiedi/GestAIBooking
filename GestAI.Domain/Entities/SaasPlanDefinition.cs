using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class SaasPlanDefinition : Entity
{
    public SaasPlanCode Code { get; set; }
    public string Name { get; set; } = null!;
    public int MaxProperties { get; set; }
    public int MaxUnits { get; set; }
    public int MaxUsers { get; set; }
    public bool IncludesReports { get; set; }
    public bool IncludesPublicPortal { get; set; }
    public bool IncludesOperations { get; set; }
}
