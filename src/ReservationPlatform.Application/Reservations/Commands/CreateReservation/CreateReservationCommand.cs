using MediatR;
using ReservationPlatform.Application.Reservations.DTOs;

namespace ReservationPlatform.Application.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    Guid RestaurantId,
    Guid TableId,
    Guid CustomerId,
    string ReservationDate,    // "yyyy-MM-dd"
    string SlotTime,           // "HH:mm"
    int PartySize,
    string? SpecialRequests
) : IRequest<ReservationDto>;
