using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationPlatform.Application.Auth.Commands.Register;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;
using ReservationPlatform.Infrastructure.Persistence;
using ReservationPlatform.Infrastructure.Persistence.Interceptors;
using ReservationPlatform.Infrastructure.Persistence.Repositories;
using ReservationPlatform.Infrastructure.Services;

namespace ReservationPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IOpeningHoursRepository, OpeningHoursRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
