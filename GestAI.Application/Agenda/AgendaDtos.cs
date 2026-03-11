using GestAI.Domain.Enums;

namespace GestAI.Application.Agenda;

public sealed record AgendaBookingDto(
    int BookingId,
    int UnitId,
    string UnitName,
    int GuestId,
    string GuestName,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    BookingStatus Status);

public sealed record DailyAgendaDto(
    DateOnly Date,
    List<AgendaBookingDto> CheckIns,
    List<AgendaBookingDto> CheckOuts,
    List<AgendaBookingDto> Next7Days);
