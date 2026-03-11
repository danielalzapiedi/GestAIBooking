namespace GestAI.Web.Dtos;

public sealed record GuestDto(
    int Id,
    int PropertyId,
    string FullName,
    string? Phone,
    string? Email,
    int? DocumentType,
    string? DocumentNumber,
    string? Notes,
    bool IsActive);

public sealed record GuestSearchItemDto(int Id, string FullName, string? Phone, string? Email);

public sealed record UpsertGuestCommand(
    int PropertyId, int? GuestId,
    string FullName, string? Phone, string? Email,
    int? DocumentType, string? DocumentNumber, string? Notes
);
