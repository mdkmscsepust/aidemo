using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Infrastructure.Persistence.Interceptors;

namespace ReservationPlatform.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    AuditableEntityInterceptor auditInterceptor) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<RestaurantTable> Tables => Set<RestaurantTable>();
    public DbSet<OpeningHours> OpeningHours => Set<OpeningHours>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
