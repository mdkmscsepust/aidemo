using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Reservations.DTOs;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandler(
    IReservationRepository reservationRepository,
    IRestaurantRepository restaurantRepository
) : IRequestHandler<GetReservationByIdQuery, ReservationDto>
{
    public async Task<ReservationDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdWithDetailsAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Reservation), request.ReservationId);

        var isAdmin = request.RequestingUserRole == "Admin";
        var isCustomer = reservation.CustomerId == request.RequestingUserId;

        if (!isAdmin && !isCustomer)
        {
            var restaurant = await restaurantRepository.GetByIdAsync(reservation.RestaurantId, cancellationToken);
            if (restaurant?.OwnerId != request.RequestingUserId)
                throw new ForbiddenException("You are not authorized to view this reservation.");
        }

        return new ReservationDto
        {
            Id = reservation.Id,
            RestaurantId = reservation.RestaurantId,
            RestaurantName = reservation.Restaurant?.Name ?? string.Empty,
            TableId = reservation.TableId,
            TableNumber = reservation.Table?.TableNumber ?? string.Empty,
            TableCapacity = reservation.Table?.Capacity ?? 0,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer != null
                ? $"{reservation.Customer.FirstName} {reservation.Customer.LastName}"
                : string.Empty,
            PartySize = reservation.PartySize,
            ReservationDate = reservation.ReservationDate.ToString("yyyy-MM-dd"),
            StartTime = reservation.StartTime.ToString("HH:mm"),
            EndTime = reservation.EndTime.ToString("HH:mm"),
            DurationMinutes = reservation.DurationMinutes,
            Status = reservation.Status.ToString(),
            SpecialRequests = reservation.SpecialRequests,
            ConfirmationCode = reservation.ConfirmationCode,
            Notes = reservation.Notes,
            CancelledAt = reservation.CancelledAt,
            CancellationReason = reservation.CancellationReason,
            CreatedAt = reservation.CreatedAt
        };
    }
}
