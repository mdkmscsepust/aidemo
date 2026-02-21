using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;

namespace ReservationPlatform.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await context.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(ct);
        foreach (var token in tokens) token.Revoke();
    }
}
