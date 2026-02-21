using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class RestaurantRepository(ApplicationDbContext context) : IRestaurantRepository
{
    public Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Restaurants.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Restaurant?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        context.Restaurants
            .Include(r => r.OpeningHours.OrderBy(h => h.DayOfWeek))
            .Include(r => r.Tables.Where(t => t.IsActive))
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(Restaurant restaurant, CancellationToken ct = default) =>
        await context.Restaurants.AddAsync(restaurant, ct);

    public async Task<(IEnumerable<Restaurant> Items, int TotalCount)> SearchAsync(
        string? search, string? city, string? cuisineType, int? priceTierMin, int? priceTierMax,
        int page, int pageSize, string? sortBy, CancellationToken ct = default)
    {
        var query = context.Restaurants.Where(r => r.IsActive && r.IsApproved).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => EF.Functions.ILike(r.Name, $"%{search}%") ||
                                     EF.Functions.ILike(r.City, $"%{search}%") ||
                                     (r.CuisineType != null && EF.Functions.ILike(r.CuisineType, $"%{search}%")));

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(r => EF.Functions.ILike(r.City, $"%{city}%"));

        if (!string.IsNullOrWhiteSpace(cuisineType))
            query = query.Where(r => r.CuisineType != null && EF.Functions.ILike(r.CuisineType, $"%{cuisineType}%"));

        if (priceTierMin.HasValue)
            query = query.Where(r => (int)r.PriceTier >= priceTierMin.Value);

        if (priceTierMax.HasValue)
            query = query.Where(r => (int)r.PriceTier <= priceTierMax.Value);

        var total = await query.CountAsync(ct);

        query = sortBy?.ToLower() switch
        {
            "name" => query.OrderBy(r => r.Name),
            "rating" => query.OrderByDescending(r => r.AvgRating),
            "reviews" => query.OrderByDescending(r => r.ReviewCount),
            _ => query.OrderByDescending(r => r.AvgRating)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default) =>
        await context.Restaurants.Where(r => r.OwnerId == ownerId && r.IsActive).ToListAsync(ct);

    public async Task<(IEnumerable<Restaurant> Items, int TotalCount)> GetAllForAdminAsync(
        bool? isApproved, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Restaurants.AsQueryable();
        if (isApproved.HasValue) query = query.Where(r => r.IsApproved == isApproved.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
