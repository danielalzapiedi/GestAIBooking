using GestAI.Domain.Enums;

namespace GestAI.Application.Templates;

public sealed record MessageTemplateDto(int Id, int PropertyId, TemplateType Type, string Name, string Body, bool IsActive);
