using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class OpeningHoursConfiguration : IEntityTypeConfiguration<OpeningHours>
{
    public void Configure(EntityTypeBuilder<OpeningHours> builder)
    {
        builder.ToTable("opening_hours");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(h => h.DayOfWeek).HasColumnName("day_of_week").IsRequired();
        builder.Property(h => h.OpenTime).HasColumnName("open_time").HasColumnType("time").IsRequired();
        builder.Property(h => h.CloseTime).HasColumnName("close_time").HasColumnType("time").IsRequired();
        builder.Property(h => h.IsClosed).HasColumnName("is_closed").HasDefaultValue(false).IsRequired();
        builder.Property(h => h.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(h => h.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(h => new { h.RestaurantId, h.DayOfWeek }).IsUnique().HasDatabaseName("uix_opening_hours");
    }
}
