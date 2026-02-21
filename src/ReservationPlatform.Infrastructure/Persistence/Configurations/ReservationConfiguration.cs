using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationPlatform.Domain.Entities;

namespace ReservationPlatform.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.RestaurantId).HasColumnName("restaurant_id").IsRequired();
        builder.Property(r => r.TableId).HasColumnName("table_id").IsRequired();
        builder.Property(r => r.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(r => r.PartySize).HasColumnName("party_size").IsRequired();
        builder.Property(r => r.ReservationDate).HasColumnName("reservation_date").HasColumnType("date").IsRequired();
        builder.Property(r => r.StartTime).HasColumnName("start_time").HasColumnType("time").IsRequired();
        builder.Property(r => r.EndTime).HasColumnName("end_time").HasColumnType("time").IsRequired();
        builder.Property(r => r.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").IsRequired();
        builder.Property(r => r.SpecialRequests).HasColumnName("special_requests").HasMaxLength(1000);
        builder.Property(r => r.ConfirmationCode).HasColumnName("confirmation_code").HasMaxLength(12).IsRequired();
        builder.Property(r => r.Notes).HasColumnName("notes");
        builder.Property(r => r.CancelledAt).HasColumnName("cancelled_at");
        builder.Property(r => r.CancellationReason).HasColumnName("cancellation_reason");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Double-booking is prevented via PostgreSQL advisory locks in the repository;
        // no optimistic concurrency token required here.

        builder.HasIndex(r => r.ConfirmationCode).IsUnique().HasDatabaseName("ix_reservations_code");
        builder.HasIndex(r => new { r.RestaurantId, r.ReservationDate, r.Status })
               .HasDatabaseName("ix_reservations_availability")
               .HasFilter("status IN (0, 1)");
        builder.HasIndex(r => new { r.CustomerId, r.ReservationDate }).HasDatabaseName("ix_reservations_customer");
        builder.HasIndex(r => new { r.TableId, r.ReservationDate }).HasDatabaseName("ix_reservations_table");

        builder.HasOne(r => r.Restaurant).WithMany()
               .HasForeignKey(r => r.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Table).WithMany(t => t.Reservations)
               .HasForeignKey(r => r.TableId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Customer).WithMany(u => u.Reservations)
               .HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Review).WithOne(rv => rv.Reservation)
               .HasForeignKey<Review>(rv => rv.ReservationId).OnDelete(DeleteBehavior.Restrict);
    }
}
