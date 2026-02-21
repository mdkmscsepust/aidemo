using MediatR;
using ReservationPlatform.Application.Restaurants.DTOs;

namespace ReservationPlatform.Application.Restaurants.Queries.GetRestaurantById;

public record GetRestaurantByIdQuery(Guid RestaurantId) : IRequest<RestaurantDetailDto>;
