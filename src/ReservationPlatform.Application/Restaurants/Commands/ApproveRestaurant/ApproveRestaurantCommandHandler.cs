using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Restaurants.Commands.ApproveRestaurant;

public class ApproveRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ApproveRestaurantCommand, RestaurantListDto>
{
    public async Task<RestaurantListDto> Handle(ApproveRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Restaurant), request.RestaurantId);

        if (request.Approve) restaurant.Approve();
        else restaurant.Reject();

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
