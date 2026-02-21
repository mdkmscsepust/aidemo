using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Enums;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class ReservationRepository(ApplicationDbContext context) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Reservation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        context.Reservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Reservation>> GetExistingForDateAsync(
        Guid restaurantId, DateOnly date, CancellationToken ct = default) =>
        await context.Reservations
            .Where(r => r.RestaurantId == restaurantId
                     && r.ReservationDate == date
                     && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
            .ToListAsync(ct);

    public async Task<(IEnumerable<Reservation> Items, int TotalCount)> GetByCustomerAsync(
        Guid customerId, ReservationStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Reservations
            .Include(r => r.Restaurant)
            .Include(r => r.Table)
            .Where(r => r.CustomerId == customerId);

        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IEnumerable<Reservation> Items, int TotalCount)> GetByRestaurantAsync(
        Guid restaurantId, DateOnly? date, ReservationStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Where(r => r.RestaurantId == restaurantId);

        if (date.HasValue) query = query.Where(r => r.ReservationDate == date.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(r => r.ReservationDate).ThenBy(r => r.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task<Reservation> CreateWithLockAsync(
        Guid restaurantId, Guid tableId, Guid customerId,
        DateOnly reservationDate, TimeOnly startTime, int durationMinutes, int partySize,
        string? specialRequests, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            try
            {
                // Acquire PostgreSQL advisory lock scoped to this transaction
                // Key = XOR of first 8 bytes of tableId UUID with date day number
                var lockKey = ComputeLockKey(tableId, reservationDate);
                await context.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock({0})", lockKey);

                var endTime = startTime.AddMinutes(durationMinutes);

                // Check for overlapping confirmed/pending reservations
                var hasConflict = await context.Reservations.AnyAsync(r =>
                    r.TableId == tableId &&
                    r.ReservationDate == reservationDate &&
                    (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                    r.StartTime < endTime &&
                    r.EndTime > startTime, ct);

                if (hasConflict)
                    throw new ConflictException("This time slot is no longer available. Please select another slot.");

                var reservation = Reservation.Create(
                    restaurantId, tableId, customerId,
                    reservationDate, startTime, durationMinutes, partySize, specialRequests);

                await context.Reservations.AddAsync(reservation, ct);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return reservation;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }

    private static long ComputeLockKey(Guid tableId, DateOnly date)
    {
        var bytes = tableId.ToByteArray();
        var tableHash = BitConverter.ToInt64(bytes, 0);
        return tableHash ^ ((long)date.DayNumber << 17);
    }
}
