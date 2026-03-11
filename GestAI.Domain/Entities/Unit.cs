using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class Unit : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int CapacityAdults { get; set; } = 2;
    public int CapacityChildren { get; set; } = 0;
    public decimal BaseRate { get; set; } = 0m;
    public int TotalCapacity { get; set; } = 2;
    public string? ShortDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public UnitOperationalStatus OperationalStatus { get; set; } = UnitOperationalStatus.Available;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<BlockedDate> BlockedDates { get; set; } = new List<BlockedDate>();
    public ICollection<RatePlan> RatePlans { get; set; } = new List<RatePlan>();
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    public ICollection<OperationalTask> Tasks { get; set; } = new List<OperationalTask>();
    public ICollection<ExternalChannelConnection> ExternalChannelConnections { get; set; } = new List<ExternalChannelConnection>();
    public ICollection<ExternalCalendarEvent> ExternalCalendarEvents { get; set; } = new List<ExternalCalendarEvent>();
}
