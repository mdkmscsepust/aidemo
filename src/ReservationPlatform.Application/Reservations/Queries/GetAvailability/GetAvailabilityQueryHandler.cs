using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Reservations.DTOs;
using ReservationPlatform.Domain.Enums;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reservations.Queries.GetAvailability;

public class GetAvailabilityQueryHandler(
    IRestaurantRepository restaurantRepository,
    IOpeningHoursRepository openingHoursRepository,
    ITableRepository tableRepository,
    IReservationRepository reservationRepository
) : IRequestHandler<GetAvailabilityQuery, List<AvailableSlotDto>>
{
    private const int SlotIntervalMinutes = 15;

    public async Task<List<AvailableSlotDto>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(request.Date, "yyyy-MM-dd", out var date))
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Date", "Date must be in yyyy-MM-dd format.")]);

        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Restaurant), request.RestaurantId);

        if (!restaurant.IsApproved || !restaurant.IsActive)
            return [];

        var hours = await openingHoursRepository.GetForDayAsync(request.RestaurantId, date.DayOfWeek, cancellationToken);
        if (hours == null || hours.IsClosed)
            return [];

        // Load tables sorted by capacity ASC for best-fit allocation
        var tables = (await tableRepository.GetAvailableTablesAsync(request.RestaurantId, request.PartySize, cancellationToken))
            .OrderBy(t => t.Capacity)
            .ToList();

        if (tables.Count == 0)
            return [];

        // Load all existing confirmed/pending reservations for this restaurant+date
        var existingReservations = (await reservationRepository.GetExistingForDateAsync(request.RestaurantId, date, cancellationToken))
            .ToList();

        var duration = restaurant.DefaultDurationMinutes;
        var availableSlots = new List<AvailableSlotDto>();

        // Generate 15-minute slots from open time to last possible start time
        var currentSlot = RoundUpToSlotBoundary(hours.OpenTime, SlotIntervalMinutes);
        var lastPossibleStart = hours.CloseTime.AddMinutes(-duration);

        while (currentSlot <= lastPossibleStart)
        {
            var slotEnd = currentSlot.AddMinutes(duration);

            // Best-fit: find smallest table that fits the party and has no overlap
            foreach (var table in tables)
            {
                if (table.MinCapacity > request.PartySize || table.Capacity < request.PartySize)
                    continue;

                var hasConflict = existingReservations.Any(r =>
                    r.TableId == table.Id &&
                    r.Status is ReservationStatus.Pending or ReservationStatus.Confirmed &&
                    r.StartTime < slotEnd &&
                    r.EndTime > currentSlot);

                if (!hasConflict)
                {
                    availableSlots.Add(new AvailableSlotDto
                    {
                        SlotTime = currentSlot.ToString("HH:mm"),
                        TableId = table.Id,
                        TableNumber = table.TableNumber,
                        TableCapacity = table.Capacity
                    });
                    break; // best-fit: take smallest suitable table
                }
            }

            currentSlot = currentSlot.AddMinutes(SlotIntervalMinutes);
        }

        return availableSlots;
    }

    private static TimeOnly RoundUpToSlotBoundary(TimeOnly time, int slotMinutes)
    {
        var totalMinutes = time.Hour * 60 + time.Minute;
        var remainder = totalMinutes % slotMinutes;
        if (remainder == 0) return time;
        var rounded = totalMinutes + (slotMinutes - remainder);
        return TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(rounded));
    }
}
