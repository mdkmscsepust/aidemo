using MediatR;
using ReservationPlatform.Application.Reservations.DTOs;

namespace ReservationPlatform.Application.Reservations.Commands.CancelReservation;

public record CancelReservationCommand(
    Guid ReservationId,
    Guid RequestingUserId,
    string RequestingUserRole,
    string? Reason
) : IRequest<ReservationDto>;
