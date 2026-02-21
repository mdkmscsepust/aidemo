using MediatR;
using ReservationPlatform.Application.Reservations.DTOs;

namespace ReservationPlatform.Application.Reservations.Queries.GetReservationById;

public record GetReservationByIdQuery(Guid ReservationId, Guid RequestingUserId, string RequestingUserRole)
    : IRequest<ReservationDto>;
