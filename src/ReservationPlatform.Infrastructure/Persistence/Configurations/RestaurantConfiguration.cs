using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.OwnerId).HasColumnName("owner_id").IsRequired();
        builder.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasColumnName("description");
        builder.Property(r => r.CuisineType).HasColumnName("cuisine_type").HasMaxLength(100);
        builder.Property(r => r.AddressLine1).HasColumnName("address_line1").HasMaxLength(255).IsRequired();
        builder.Property(r => r.AddressLine2).HasColumnName("address_line2").HasMaxLength(255);
        builder.Property(r => r.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        builder.Property(r => r.State).HasColumnName("state").HasMaxLength(100);
        builder.Property(r => r.PostalCode).HasColumnName("postal_code").HasMaxLength(20).IsRequired();
        builder.Property(r => r.Country).HasColumnName("country").HasMaxLength(100).HasDefaultValue("US").IsRequired();
        builder.Property(r => r.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(r => r.Email).HasColumnName("email").HasMaxLength(320);
        builder.Property(r => r.Website).HasColumnName("website").HasMaxLength(500);
        builder.Property(r => r.Latitude).HasColumnName("latitude").HasColumnType("numeric(10,7)");
        builder.Property(r => r.Longitude).HasColumnName("longitude").HasColumnType("numeric(10,7)");
        builder.Property(r => r.AvgRating).HasColumnName("avg_rating").HasColumnType("numeric(3,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(r => r.ReviewCount).HasColumnName("review_count").HasDefaultValue(0).IsRequired();
        builder.Property(r => r.PriceTier).HasColumnName("price_tier").IsRequired();
        builder.Property(r => r.DefaultDurationMinutes).HasColumnName("default_duration_minutes").HasDefaultValue(90).IsRequired();
        builder.Property(r => r.IsApproved).HasColumnName("is_approved").HasDefaultValue(false).IsRequired();
        builder.Property(r => r.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
        builder.Property(r => r.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(r => r.OwnerId).HasDatabaseName("ix_restaurants_owner");
        builder.HasIndex(r => r.City).HasDatabaseName("ix_restaurants_city");
        builder.HasIndex(r => r.CuisineType).HasDatabaseName("ix_restaurants_cuisine");

        builder.HasOne(r => r.Owner).WithMany()
               .HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Tables).WithOne(t => t.Restaurant)
               .HasForeignKey(t => t.RestaurantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.OpeningHours).WithOne(h => h.Restaurant)
               .HasForeignKey(h => h.RestaurantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.Reviews).WithOne(rv => rv.Restaurant)
               .HasForeignKey(rv => rv.RestaurantId).OnDelete(DeleteBehavior.Cascade);
    }
}
