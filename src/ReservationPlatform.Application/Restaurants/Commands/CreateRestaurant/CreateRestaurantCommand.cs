using MediatR;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Application.Restaurants.Commands.CreateRestaurant;

public record CreateRestaurantCommand(
    Guid OwnerId,
    string Name,
    string? Description,
    string? CuisineType,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string PostalCode,
    string Country,
    string? Phone,
    string? Email,
    PriceTier PriceTier,
    int DefaultDurationMinutes
) : IRequest<RestaurantListDto>;
