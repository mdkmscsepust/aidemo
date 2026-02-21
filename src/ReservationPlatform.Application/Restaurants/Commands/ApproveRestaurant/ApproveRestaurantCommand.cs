using MediatR;
using ReservationPlatform.Application.Restaurants.DTOs;

namespace ReservationPlatform.Application.Restaurants.Commands.ApproveRestaurant;

public record ApproveRestaurantCommand(Guid RestaurantId, bool Approve) : IRequest<RestaurantListDto>;
