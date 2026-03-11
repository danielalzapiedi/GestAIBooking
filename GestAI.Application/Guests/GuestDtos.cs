namespace GestAI.Application.Guests;

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
