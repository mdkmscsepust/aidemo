using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Domain.Interfaces.Repositories;

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Restaurant?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Restaurant restaurant, CancellationToken ct = default);
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? search,
        string? city,
        string? cuisineType,
        int? priceTierMin,
        int? priceTierMax,
        int page,
        int pageSize,
        string? sortBy,
        CancellationToken ct = default);
    Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<(IEnumerable<Restaurant> Items, int TotalCount)> GetAllForAdminAsync(
        bool? isApproved, int page, int pageSize, CancellationToken ct = default);
}
