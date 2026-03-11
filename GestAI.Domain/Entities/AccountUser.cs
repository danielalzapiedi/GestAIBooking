using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class AccountUser : Entity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    public InternalUserRole Role { get; set; } = InternalUserRole.Reception;
    public bool CanManageBookings { get; set; } = true;
    public bool CanManageGuests { get; set; } = true;
    public bool CanManagePayments { get; set; } = false;
    public bool CanViewReports { get; set; } = false;
    public bool CanManageConfiguration { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime InvitedAtUtc { get; set; } = DateTime.UtcNow;
}
