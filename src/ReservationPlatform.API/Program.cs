using Microsoft.EntityFrameworkCore;
using AspNetCoreRateLimit;
using ReservationPlatform.API.Extensions;
using ReservationPlatform.API.Middleware;
using ReservationPlatform.Application;
using ReservationPlatform.Infrastructure;
using ReservationPlatform.Infrastructure.Persistence;
using ReservationPlatform.Infrastructure.Persistence.Seeders;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddRateLimitingServices(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reservation API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

app.UseCors("DefaultPolicy");
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (app.Environment.IsDevelopment())
        {
            await DataSeeder.SeedAsync(context);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database.");
    }
}

Log.Information("Restaurant Reservation API starting up...");
app.Run();
