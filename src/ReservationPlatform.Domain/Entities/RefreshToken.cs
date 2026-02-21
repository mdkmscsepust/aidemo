using ReservationPlatform.Domain.Common;

namespace ReservationPlatform.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedById { get; private set; }

    public User? User { get; private set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    public bool IsRevoked => RevokedAt != null;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke(Guid? replacedById = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedById = replacedById;
    }
}
