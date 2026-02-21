using MediatR;

namespace ReservationPlatform.Application.Restaurants.Commands.DeleteRestaurant;

public record DeleteRestaurantCommand(Guid RestaurantId, Guid RequestingUserId, string RequestingUserRole) : IRequest;
