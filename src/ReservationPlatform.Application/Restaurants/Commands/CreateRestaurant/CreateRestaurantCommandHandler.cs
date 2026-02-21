using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateRestaurantCommand, RestaurantListDto>
{
    public async Task<RestaurantListDto> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var owner = await userRepository.GetByIdAsync(request.OwnerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), request.OwnerId);

        var restaurant = Restaurant.Create(
            request.OwnerId, request.Name, request.Description, request.CuisineType,
            request.AddressLine1, request.AddressLine2, request.City, request.State,
            request.PostalCode, request.Country, request.Phone, request.Email,
            request.PriceTier, request.DefaultDurationMinutes);

        await restaurantRepository.AddAsync(restaurant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RestaurantListDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            CuisineType = restaurant.CuisineType,
            City = restaurant.City,
            AddressLine1 = restaurant.AddressLine1,
            AvgRating = restaurant.AvgRating,
            ReviewCount = restaurant.ReviewCount,
            PriceTier = restaurant.PriceTier.ToString(),
            IsApproved = restaurant.IsApproved
        };
    }
}
