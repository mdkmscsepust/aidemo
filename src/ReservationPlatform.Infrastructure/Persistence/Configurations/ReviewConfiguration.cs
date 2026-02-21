using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(r => r.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(r => r.ReservationId).HasColumnName("reservation_id").IsRequired();
        builder.Property(r => r.Rating).HasColumnName("rating").IsRequired();
        builder.Property(r => r.Comment).HasColumnName("comment");
        builder.Property(r => r.IsPublished).HasColumnName("is_published").HasDefaultValue(true).IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(r => r.ReservationId).IsUnique().HasDatabaseName("uix_one_review_per_reservation");
        builder.HasIndex(r => new { r.RestaurantId, r.CreatedAt }).HasDatabaseName("ix_reviews_restaurant")
               .HasFilter("is_published = true");
        builder.HasIndex(r => r.CustomerId).HasDatabaseName("ix_reviews_customer");
    }
}
