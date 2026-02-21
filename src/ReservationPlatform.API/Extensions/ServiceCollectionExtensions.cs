using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ReservationPlatform.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync("{\"success\":false,\"message\":\"Authentication required.\"}");
                    },
                    OnForbidden = ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync("{\"success\":false,\"message\":\"Insufficient permissions.\"}");
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));
            options.AddPolicy("OwnerOnly", p => p.RequireRole("Owner"));
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("OwnerOrAdmin", p => p.RequireRole("Owner", "Admin"));
            options.AddPolicy("Authenticated", p => p.RequireAuthenticatedUser());
        });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Restaurant Reservation API",
                Version = "v1",
                Description = "Production-grade restaurant reservation platform API"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Enter: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(config.GetSection("IpRateLimiting"));
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        services.AddInMemoryRateLimiting();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200"];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
                builder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        return services;
    }
}
