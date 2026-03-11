namespace GestAI.Web.Dtos;

public sealed record MessageTemplateDto(int Id, int PropertyId, TemplateType Type, string Name, string Body, bool IsActive);
public sealed record UpsertTemplateCommand(int PropertyId, int? TemplateId, TemplateType Type, string Name, string Body, bool IsActive);
