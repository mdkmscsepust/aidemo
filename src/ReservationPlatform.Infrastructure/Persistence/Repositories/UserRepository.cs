using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Enums;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        context.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await context.Users.AddAsync(user, ct);

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? role, CancellationToken ct = default)
    {
        var query = context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
            query = query.Where(u => u.Role == userRole);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }
}
