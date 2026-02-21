using MediatR;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Reservations.DTOs;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Application.Reservations.Queries.GetMyReservations;

public record GetMyReservationsQuery(
    Guid CustomerId,
    ReservationStatus? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<ReservationDto>>;
