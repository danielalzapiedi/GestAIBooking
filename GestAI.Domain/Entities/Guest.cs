using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class Guest : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public int? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
