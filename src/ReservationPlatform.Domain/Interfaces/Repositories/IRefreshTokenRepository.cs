using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAllByUserAsync(Guid userId, CancellationToken ct = default);
}
