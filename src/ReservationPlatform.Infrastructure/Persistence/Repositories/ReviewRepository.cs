using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Review?> GetByReservationIdAsync(Guid reservationId, CancellationToken ct = default) =>
        context.Reviews.FirstOrDefaultAsync(r => r.ReservationId == reservationId, ct);

    public async Task<(IEnumerable<Review> Items, int TotalCount)> GetByRestaurantAsync(
        Guid restaurantId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Reviews
            .Include(r => r.Customer)
            .Where(r => r.RestaurantId == restaurantId && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Review review, CancellationToken ct = default) =>
        await context.Reviews.AddAsync(review, ct);

    public async Task<(decimal AvgRating, int Count)> GetRatingStatsAsync(Guid restaurantId, CancellationToken ct = default)
    {
        var stats = await context.Reviews
            .Where(r => r.RestaurantId == restaurantId && r.IsPublished)
            .GroupBy(_ => true)
            .Select(g => new { Avg = (decimal)g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return stats == null ? (0m, 0) : (Math.Round(stats.Avg, 2), stats.Count);
    }
}
