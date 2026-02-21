using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Domain.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Reservation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<IEnumerable<Reservation>> GetExistingForDateAsync(
        Guid restaurantId, DateOnly date, CancellationToken ct = default);

    Task<(IEnumerable<Reservation> Items, int TotalCount)> GetByCustomerAsync(
        Guid customerId, ReservationStatus? status, int page, int pageSize, CancellationToken ct = default);

    Task<(IEnumerable<Reservation> Items, int TotalCount)> GetByRestaurantAsync(
        Guid restaurantId, DateOnly? date, ReservationStatus? status, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Creates a reservation atomically with advisory lock + overlap check.
    /// Throws ConflictException if the slot is already taken.
    /// </summary>
    Task<Reservation> CreateWithLockAsync(
        Guid restaurantId,
        Guid tableId,
        Guid customerId,
        DateOnly reservationDate,
        TimeOnly startTime,
        int durationMinutes,
        int partySize,
        string? specialRequests,
        CancellationToken ct = default);
}
