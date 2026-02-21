using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Reservations.DTOs;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler(
    IRestaurantRepository restaurantRepository,
    ITableRepository tableRepository,
    IOpeningHoursRepository openingHoursRepository,
    IReservationRepository reservationRepository
) : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var date = DateOnly.ParseExact(request.ReservationDate, "yyyy-MM-dd");
        var slotTime = TimeOnly.ParseExact(request.SlotTime, "HH:mm");

        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Restaurant), request.RestaurantId);

        if (!restaurant.IsApproved || !restaurant.IsActive)
            throw new ConflictException("Restaurant is not accepting reservations.");

        var table = await tableRepository.GetByIdAsync(request.TableId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.RestaurantTable), request.TableId);

        if (table.RestaurantId != request.RestaurantId)
            throw new ConflictException("Table does not belong to this restaurant.");

        if (!table.IsActive)
            throw new ConflictException("Table is not available.");

        if (table.Capacity < request.PartySize || table.MinCapacity > request.PartySize)
            throw new ConflictException($"Party size {request.PartySize} does not fit table capacity {table.MinCapacity}-{table.Capacity}.");

        var hours = await openingHoursRepository.GetForDayAsync(request.RestaurantId, date.DayOfWeek, cancellationToken);
        if (hours == null || hours.IsClosed)
            throw new ConflictException("Restaurant is closed on this day.");

        var slotEnd = slotTime.AddMinutes(restaurant.DefaultDurationMinutes);
        if (slotTime < hours.OpenTime || slotEnd > hours.CloseTime)
            throw new ConflictException("Requested time is outside opening hours.");

        var reservation = await reservationRepository.CreateWithLockAsync(
            request.RestaurantId, request.TableId, request.CustomerId,
            date, slotTime, restaurant.DefaultDurationMinutes, request.PartySize,
            request.SpecialRequests, cancellationToken);

        return MapToDto(reservation, restaurant.Name, table.TableNumber, table.Capacity);
    }

    private static ReservationDto MapToDto(
        Domain.Entities.Reservation r, string restaurantName, string tableNumber, int tableCapacity) =>
        new()
        {
            Id = r.Id,
            RestaurantId = r.RestaurantId,
            RestaurantName = restaurantName,
            TableId = r.TableId,
            TableNumber = tableNumber,
            TableCapacity = tableCapacity,
            CustomerId = r.CustomerId,
            CustomerName = string.Empty,
            PartySize = r.PartySize,
            ReservationDate = r.ReservationDate.ToString("yyyy-MM-dd"),
            StartTime = r.StartTime.ToString("HH:mm"),
            EndTime = r.EndTime.ToString("HH:mm"),
            DurationMinutes = r.DurationMinutes,
            Status = r.Status.ToString(),
            SpecialRequests = r.SpecialRequests,
            ConfirmationCode = r.ConfirmationCode,
            Notes = r.Notes,
            CancelledAt = r.CancelledAt,
            CancellationReason = r.CancellationReason,
            CreatedAt = r.CreatedAt
        };
}
