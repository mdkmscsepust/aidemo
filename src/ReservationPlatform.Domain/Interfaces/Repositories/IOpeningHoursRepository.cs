using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Domain.Interfaces.Repositories;

public interface IOpeningHoursRepository
{
    Task<OpeningHours?> GetForDayAsync(Guid restaurantId, DayOfWeek dayOfWeek, CancellationToken ct = default);
    Task<IEnumerable<OpeningHours>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default);
    Task UpsertAsync(IEnumerable<OpeningHours> openingHours, CancellationToken ct = default);
}
