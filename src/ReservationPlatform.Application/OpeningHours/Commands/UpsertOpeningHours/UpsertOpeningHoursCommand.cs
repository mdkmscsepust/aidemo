using MediatR;
using ReservationPlatform.Application.OpeningHours.DTOs;

namespace ReservationPlatform.Application.OpeningHours.Commands.UpsertOpeningHours;

public record UpsertOpeningHoursCommand(
    Guid RestaurantId,
    Guid RequestingUserId,
    string RequestingUserRole,
    List<OpeningHoursInputDto> Hours
) : IRequest;
