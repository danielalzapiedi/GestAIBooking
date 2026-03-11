namespace GestAI.Web.Dtos;

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


public enum SavedQuoteStatus
{
    Draft = 0,
    Saved = 1,
    Converted = 2,
    Expired = 3,
    Cancelled = 4
}
