using MediatR;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Application.Restaurants.Commands.UpdateRestaurant;

public record UpdateRestaurantCommand(
    Guid RestaurantId,
    Guid RequestingUserId,
    string RequestingUserRole,
    string Name,
    string? Description,
    string? CuisineType,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string PostalCode,
    string? Phone,
    string? Email,
    string? Website,
    PriceTier PriceTier,
    int DefaultDurationMinutes,
    string? ImageUrl
) : IRequest<RestaurantListDto>;
