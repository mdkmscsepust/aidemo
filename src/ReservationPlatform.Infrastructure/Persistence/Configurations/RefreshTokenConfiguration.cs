using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.Property(t => t.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(t => t.RevokedAt).HasColumnName("revoked_at");
        builder.Property(t => t.ReplacedById).HasColumnName("replaced_by");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.Ignore(t => t.IsActive);
        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsRevoked);

        builder.HasIndex(t => t.Token).IsUnique().HasDatabaseName("uix_refresh_tokens_token");
        builder.HasIndex(t => t.UserId).HasDatabaseName("ix_refresh_tokens_user");
    }
}
