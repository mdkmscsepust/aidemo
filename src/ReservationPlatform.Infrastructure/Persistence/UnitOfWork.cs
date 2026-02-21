using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
