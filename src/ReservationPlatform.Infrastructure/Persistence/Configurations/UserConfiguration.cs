using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(100).IsRequired();
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(u => u.Role).HasColumnName("role").IsRequired();
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("uix_users_email");

        builder.HasMany(u => u.Reservations).WithOne(r => r.Customer)
               .HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(u => u.Reviews).WithOne(r => r.Customer)
               .HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(u => u.RefreshTokens).WithOne(t => t.User)
               .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
