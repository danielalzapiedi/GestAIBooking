namespace GestAI.Web.Dtos;

public sealed record AppResult(bool Success, string? ErrorCode, string? Message);
public sealed record AppResult<T>(bool Success, T? Data, string? ErrorCode, string? Message);
