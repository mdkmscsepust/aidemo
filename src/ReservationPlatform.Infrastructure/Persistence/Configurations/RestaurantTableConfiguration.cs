using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable("tables");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(t => t.TableNumber).HasColumnName("table_number").HasMaxLength(20).IsRequired();
        builder.Property(t => t.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(t => t.MinCapacity).HasColumnName("min_capacity").HasDefaultValue(1).IsRequired();
        builder.Property(t => t.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(t => t.Notes).HasColumnName("notes");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(t => new { t.RestaurantId, t.TableNumber }).IsUnique().HasDatabaseName("uix_tables_number");
        builder.HasIndex(t => t.RestaurantId).HasDatabaseName("ix_tables_restaurant");

        builder.HasMany(t => t.Reservations).WithOne(r => r.Table)
               .HasForeignKey(r => r.TableId).OnDelete(DeleteBehavior.Restrict);
    }
}
