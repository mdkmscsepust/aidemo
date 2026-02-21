using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateRestaurantCommand, RestaurantListDto>
{
    public async Task<RestaurantListDto> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Restaurant), request.RestaurantId);

        if (request.RequestingUserRole != "Admin" && restaurant.OwnerId != request.RequestingUserId)
            throw new ForbiddenException("You do not own this restaurant.");

        restaurant.Update(
            request.Name, request.Description, request.CuisineType,
            request.AddressLine1, request.AddressLine2, request.City, request.State,
            request.PostalCode, request.Phone, request.Email, request.Website,
            request.PriceTier, request.DefaultDurationMinutes, request.ImageUrl);

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
            IsApproved = restaurant.IsApproved,
            ImageUrl = restaurant.ImageUrl
        };
    }
}
