using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class TableRepository(ApplicationDbContext context) : ITableRepository
{
    public Task<RestaurantTable?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Tables.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IEnumerable<RestaurantTable>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default) =>
        await context.Tables.Where(t => t.RestaurantId == restaurantId).OrderBy(t => t.Capacity).ToListAsync(ct);

    public async Task<IEnumerable<RestaurantTable>> GetAvailableTablesAsync(
        Guid restaurantId, int minCapacity, CancellationToken ct = default) =>
        await context.Tables
            .Where(t => t.RestaurantId == restaurantId && t.IsActive && t.Capacity >= minCapacity)
            .OrderBy(t => t.Capacity)
            .ToListAsync(ct);

    public async Task AddAsync(RestaurantTable table, CancellationToken ct = default) =>
        await context.Tables.AddAsync(table, ct);
}
