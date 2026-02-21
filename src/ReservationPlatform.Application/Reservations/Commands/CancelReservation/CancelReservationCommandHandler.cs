using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Reservations.DTOs;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandler(
    IReservationRepository reservationRepository,
    IRestaurantRepository restaurantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelReservationCommand, ReservationDto>
{
    public async Task<ReservationDto> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdWithDetailsAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Reservation), request.ReservationId);

        var isAdmin = request.RequestingUserRole == "Admin";
        var isOwner = request.RequestingUserRole == "Owner";
        var isCustomer = reservation.CustomerId == request.RequestingUserId;

        if (!isAdmin && !isCustomer)
        {
            // Check if owner of the restaurant
            var restaurant = await restaurantRepository.GetByIdAsync(reservation.RestaurantId, cancellationToken);
            if (restaurant?.OwnerId != request.RequestingUserId)
                throw new ForbiddenException("You are not authorized to cancel this reservation.");
            isOwner = true;
        }

        if (isCustomer && !isAdmin)
        {
            if (!reservation.CanBeCancelledByCustomer())
                throw new ConflictException("Reservations can only be cancelled at least 2 hours in advance.");
        }

        reservation.Cancel(request.Reason, byRestaurant: isOwner && !isCustomer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReservationDto
        {
            Id = reservation.Id,
            RestaurantId = reservation.RestaurantId,
            RestaurantName = reservation.Restaurant?.Name ?? string.Empty,
            TableId = reservation.TableId,
            TableNumber = reservation.Table?.TableNumber ?? string.Empty,
            TableCapacity = reservation.Table?.Capacity ?? 0,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer != null ? $"{reservation.Customer.FirstName} {reservation.Customer.LastName}" : string.Empty,
            PartySize = reservation.PartySize,
            ReservationDate = reservation.ReservationDate.ToString("yyyy-MM-dd"),
            StartTime = reservation.StartTime.ToString("HH:mm"),
            EndTime = reservation.EndTime.ToString("HH:mm"),
            DurationMinutes = reservation.DurationMinutes,
            Status = reservation.Status.ToString(),
            SpecialRequests = reservation.SpecialRequests,
            ConfirmationCode = reservation.ConfirmationCode,
            CancelledAt = reservation.CancelledAt,
            CancellationReason = reservation.CancellationReason,
            CreatedAt = reservation.CreatedAt
        };
    }
}
