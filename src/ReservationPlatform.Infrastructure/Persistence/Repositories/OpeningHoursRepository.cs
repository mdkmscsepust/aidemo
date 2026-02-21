using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class OpeningHoursRepository(ApplicationDbContext context) : IOpeningHoursRepository
{
    public Task<OpeningHours?> GetForDayAsync(Guid restaurantId, DayOfWeek dayOfWeek, CancellationToken ct = default) =>
        context.OpeningHours.FirstOrDefaultAsync(
            h => h.RestaurantId == restaurantId && h.DayOfWeek == dayOfWeek, ct);

    public async Task<IEnumerable<OpeningHours>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default) =>
        await context.OpeningHours.Where(h => h.RestaurantId == restaurantId)
            .OrderBy(h => h.DayOfWeek).ToListAsync(ct);

    public async Task UpsertAsync(IEnumerable<OpeningHours> openingHours, CancellationToken ct = default)
    {
        var list = openingHours.ToList();
        if (list.Count == 0) return;

        var restaurantId = list[0].RestaurantId;
        var existing = await context.OpeningHours
            .Where(h => h.RestaurantId == restaurantId).ToListAsync(ct);

        foreach (var newHours in list)
        {
            var existingEntry = existing.FirstOrDefault(e => e.DayOfWeek == newHours.DayOfWeek);
            if (existingEntry != null)
            {
                existingEntry.Update(newHours.OpenTime, newHours.CloseTime, newHours.IsClosed);
            }
            else
            {
                await context.OpeningHours.AddAsync(newHours, ct);
            }
        }
    }
}
