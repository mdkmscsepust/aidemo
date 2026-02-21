using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using DomainOpeningHours = ReservationPlatform.Domain.Entities.OpeningHours;
using DomainRestaurant = ReservationPlatform.Domain.Entities.Restaurant;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.OpeningHours.Commands.UpsertOpeningHours;

public class UpsertOpeningHoursCommandHandler(
    IRestaurantRepository restaurantRepository,
    IOpeningHoursRepository openingHoursRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpsertOpeningHoursCommand>
{
    public async Task Handle(UpsertOpeningHoursCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(DomainRestaurant), request.RestaurantId);

        if (request.RequestingUserRole != "Admin" && restaurant.OwnerId != request.RequestingUserId)
            throw new ForbiddenException("You do not own this restaurant.");

        var openingHours = request.Hours.Select(h =>
        {
            if (!Enum.TryParse<DayOfWeek>(h.DayOfWeek, true, out var dayOfWeek))
                throw new ValidationException([new FluentValidation.Results.ValidationFailure("DayOfWeek", $"Invalid day: {h.DayOfWeek}")]);

            return DomainOpeningHours.Create(
                request.RestaurantId,
                dayOfWeek,
                TimeOnly.ParseExact(h.OpenTime, "HH:mm"),
                TimeOnly.ParseExact(h.CloseTime, "HH:mm"),
                h.IsClosed);
        }).ToList();

        await openingHoursRepository.UpsertAsync(openingHours, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
