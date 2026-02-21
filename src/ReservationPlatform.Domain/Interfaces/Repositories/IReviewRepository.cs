using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Domain.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Review?> GetByReservationIdAsync(Guid reservationId, CancellationToken ct = default);
    Task<(IEnumerable<Review> Items, int TotalCount)> GetByRestaurantAsync(
        Guid restaurantId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    Task<(decimal AvgRating, int Count)> GetRatingStatsAsync(Guid restaurantId, CancellationToken ct = default);
}
