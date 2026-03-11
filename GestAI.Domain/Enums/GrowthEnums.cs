namespace GestAI.Domain.Enums;

public enum DiscountValueType
{
    Fixed = 0,
    Percentage = 1
}

public enum PromotionScope
{
    EntireStay = 0,
    PerNight = 1
}

public enum BookingOperationalStatus
{
    PendingCheckIn = 0,
    CheckedIn = 1,
    PendingCheckOut = 2,
    CheckedOut = 3
}

public enum BookingEventType
{
    Created = 0,
    Quoted = 1,
    StatusChanged = 2,
    PaymentRegistered = 3,
    CheckIn = 4,
    CheckOut = 5,
    Cancelled = 6,
    NoteAdded = 7,
    Audit = 8
}

public enum OperationalTaskType
{
    Cleaning = 0,
    Maintenance = 1,
    Restock = 2,
    Inspection = 3,
    Other = 99
}

public enum OperationalTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum OperationalTaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}

public enum InternalUserRole
{
    Owner = 0,
    Admin = 1,
    Reception = 2,
    Operations = 3
}

public enum SaasPlanCode
{
    Starter = 0,
    Pro = 1,
    Manager = 2
}


public enum SavedQuoteStatus
{
    Draft = 0,
    Saved = 1,
    Converted = 2,
    Expired = 3,
    Cancelled = 4
}


public enum SaasModule
{
    Dashboard = 0,
    Properties = 1,
    Units = 2,
    Rates = 3,
    Promotions = 4,
    Bookings = 5,
    Guests = 6,
    Payments = 7,
    Housekeeping = 8,
    Reports = 9,
    Users = 10,
    Configuration = 11
}


public enum ExternalChannelType
{
    Booking = 0,
    Airbnb = 1,
    Other = 99
}

public enum ExternalSyncStatus
{
    Never = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}
