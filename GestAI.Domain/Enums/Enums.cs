namespace GestAI.Domain.Enums;

public enum BookingStatus
{
    Inquiry = 0,
    Tentative = 1,
    Confirmed = 2,
    CheckedIn = 3,
    CheckedOut = 4,
    Cancelled = 5
}

public enum BookingSource
{
    Direct = 0,
    WhatsApp = 1,
    Booking = 2,
    Airbnb = 3,
    Phone = 4,
    Other = 99
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1
}

public enum PaymentMethod
{
    Cash = 0,
    Transfer = 1,
    Card = 2,
    Other = 99
}

public enum UnitOperationalStatus
{
    Available = 0,
    Occupied = 1,
    PendingCleaning = 2,
    Cleaning = 3,
    Clean = 4,
    Maintenance = 5
}

public enum TemplateType
{
    Inquiry = 0,
    Confirmation = 1,
    PreCheckIn = 2,
    PostCheckOut = 3
}

public enum RateAdjustmentType
{
    Fixed = 0,
    Percentage = 1
}
