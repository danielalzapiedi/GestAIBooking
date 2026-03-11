using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class MessageTemplate : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public TemplateType Type { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = "General";
    public string Body { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
