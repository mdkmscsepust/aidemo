using MediatR;
using ReservationPlatform.Application.Reservations.DTOs;

namespace ReservationPlatform.Application.Reservations.Queries.GetAvailability;

public record GetAvailabilityQuery(Guid RestaurantId, string Date, int PartySize)
    : IRequest<List<AvailableSlotDto>>;
