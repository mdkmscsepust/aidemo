using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Domain.Interfaces.Repositories;

public interface ITableRepository
{
    Task<RestaurantTable?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<RestaurantTable>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default);
    Task<IEnumerable<RestaurantTable>> GetAvailableTablesAsync(
        Guid restaurantId, int minCapacity, CancellationToken ct = default);
    Task AddAsync(RestaurantTable table, CancellationToken ct = default);
}
