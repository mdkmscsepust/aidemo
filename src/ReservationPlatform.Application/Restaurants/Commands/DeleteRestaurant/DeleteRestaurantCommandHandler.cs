using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Restaurants.Commands.DeleteRestaurant;

public class DeleteRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteRestaurantCommand>
{
    public async Task Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Restaurant), request.RestaurantId);

        if (request.RequestingUserRole != "Admin" && restaurant.OwnerId != request.RequestingUserId)
            throw new ForbiddenException("You do not own this restaurant.");

        restaurant.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
