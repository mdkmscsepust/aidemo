using Microsoft.EntityFrameworkCore;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Enums;
using ReservationPlatform.Infrastructure.Persistence;
using ReservationPlatform.Infrastructure.Services;

namespace ReservationPlatform.Infrastructure.Persistence.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync()) return; // Already seeded

        var hasher = new PasswordHasher();

        // Users
        var admin = User.Create("admin@demo.com", hasher.Hash("Admin123!"), "Admin", "User", null, UserRole.Admin);
        var owner1 = User.Create("owner1@demo.com", hasher.Hash("Owner123!"), "Marco", "Rossi", "+1-555-0101", UserRole.Owner);
        var owner2 = User.Create("owner2@demo.com", hasher.Hash("Owner123!"), "Sophie", "Chen", "+1-555-0102", UserRole.Owner);
        var customer1 = User.Create("alice@demo.com", hasher.Hash("Customer123!"), "Alice", "Johnson", "+1-555-0201", UserRole.Customer);
        var customer2 = User.Create("bob@demo.com", hasher.Hash("Customer123!"), "Bob", "Smith", "+1-555-0202", UserRole.Customer);
        var customer3 = User.Create("carol@demo.com", hasher.Hash("Customer123!"), "Carol", "Williams", "+1-555-0203", UserRole.Customer);

        await context.Users.AddRangeAsync(admin, owner1, owner2, customer1, customer2, customer3);
        await context.SaveChangesAsync();

        // Restaurants
        var rest1 = Restaurant.Create(owner1.Id, "La Bella Italia", "Authentic Italian cuisine in the heart of the city.",
            "Italian", "123 Main Street", null, "Boston", "MA", "02101", "US",
            "+1-617-555-0001", "info@labellaitalia.com", PriceTier.Upscale, 90);
        rest1.Approve();

        var rest2 = Restaurant.Create(owner1.Id, "Sakura Garden", "Fresh sushi and traditional Japanese dishes.",
            "Japanese", "456 Harbor Ave", "Suite 2", "Boston", "MA", "02110", "US",
            "+1-617-555-0002", "hello@sakuragarden.com", PriceTier.Moderate, 75);
        rest2.Approve();

        var rest3 = Restaurant.Create(owner2.Id, "The Burger Joint", "Gourmet burgers and craft beers.",
            "American", "789 Newbury St", null, "Boston", "MA", "02116", "US",
            "+1-617-555-0003", null, PriceTier.Budget, 60);
        rest3.Approve();

        // Dhaka restaurants
        var dhakaRest1 = Restaurant.Create(owner1.Id, "Kacchi Bhai", "Legendary kacchi biryani and traditional Bangladeshi feasts.",
            "Bangladeshi", "12 Elephant Road", null, "Dhaka", "Dhaka", "1205", "BD",
            "+880-2-555-0101", "info@kacchibhai.com", PriceTier.Upscale, 90);
        dhakaRest1.Approve();
        dhakaRest1.UpdateRating(4.8m, 124);

        var dhakaRest2 = Restaurant.Create(owner2.Id, "Star Kabab & BBQ", "Mughlai kababs, tikka and tandoori cooked over live charcoal.",
            "Mughlai", "45 Dhanmondi Road 27", null, "Dhaka", "Dhaka", "1209", "BD",
            "+880-2-555-0102", null, PriceTier.Moderate, 75);
        dhakaRest2.Approve();
        dhakaRest2.UpdateRating(4.5m, 89);

        var dhakaRest3 = Restaurant.Create(owner1.Id, "Spice Garden", "Upscale Indian and Bangladeshi fine dining with a modern twist.",
            "Fine Dining", "8 Gulshan Avenue, Level 3", null, "Dhaka", "Dhaka", "1212", "BD",
            "+880-2-555-0103", "reserve@spicegarden.com", PriceTier.Fine, 105);
        dhakaRest3.Approve();
        dhakaRest3.UpdateRating(4.6m, 57);

        var dhakaRest4 = Restaurant.Create(owner2.Id, "Bengal Fusion", "Contemporary fusion of Bangladeshi flavours with global cuisine.",
            "Fusion", "22 Banani 11, Block C", null, "Dhaka", "Dhaka", "1213", "BD",
            "+880-2-555-0104", "hello@bengalfusion.com", PriceTier.Upscale, 90);
        dhakaRest4.Approve();
        dhakaRest4.UpdateRating(4.3m, 41);

        var dhakaRest5 = Restaurant.Create(owner1.Id, "Old Dhaka Kitchen", "Authentic Puran Dhaka street food — halim, bakarkhani and more.",
            "Bangladeshi", "67 Chawkbazar Lane", null, "Dhaka", "Dhaka", "1100", "BD",
            "+880-2-555-0105", null, PriceTier.Budget, 60);
        dhakaRest5.Approve();
        dhakaRest5.UpdateRating(4.7m, 203);

        await context.Restaurants.AddRangeAsync(rest1, rest2, rest3,
            dhakaRest1, dhakaRest2, dhakaRest3, dhakaRest4, dhakaRest5);
        await context.SaveChangesAsync();

        // Opening Hours (Mon-Sun for each restaurant)
        var allDays = Enum.GetValues<DayOfWeek>();
        foreach (var restaurant in new[] { rest1, rest2, rest3, dhakaRest1, dhakaRest2, dhakaRest3, dhakaRest4, dhakaRest5 })
        {
            var hours = allDays.Select(day =>
                OpeningHours.Create(restaurant.Id, day,
                    day == DayOfWeek.Sunday ? new TimeOnly(12, 0) : new TimeOnly(11, 30),
                    day == DayOfWeek.Friday || day == DayOfWeek.Saturday ? new TimeOnly(23, 0) : new TimeOnly(22, 0),
                    isClosed: false));
            await context.OpeningHours.AddRangeAsync(hours);
        }
        await context.SaveChangesAsync();

        // Tables
        var tablesRest1 = new[]
        {
            RestaurantTable.Create(rest1.Id, "T1", 2, 1, "Window seat"),
            RestaurantTable.Create(rest1.Id, "T2", 2, 1),
            RestaurantTable.Create(rest1.Id, "T3", 4, 2),
            RestaurantTable.Create(rest1.Id, "T4", 4, 2),
            RestaurantTable.Create(rest1.Id, "T5", 6, 4, "Private corner"),
            RestaurantTable.Create(rest1.Id, "T6", 8, 6, "Private room")
        };

        var tablesRest2 = new[]
        {
            RestaurantTable.Create(rest2.Id, "A1", 2, 1),
            RestaurantTable.Create(rest2.Id, "A2", 2, 1),
            RestaurantTable.Create(rest2.Id, "A3", 4, 2),
            RestaurantTable.Create(rest2.Id, "A4", 4, 2),
            RestaurantTable.Create(rest2.Id, "B1", 6, 4)
        };

        var tablesRest3 = new[]
        {
            RestaurantTable.Create(rest3.Id, "B1", 2, 1),
            RestaurantTable.Create(rest3.Id, "B2", 4, 2),
            RestaurantTable.Create(rest3.Id, "B3", 4, 2),
            RestaurantTable.Create(rest3.Id, "B4", 6, 4)
        };

        var tablesDhaka1 = new[]
        {
            RestaurantTable.Create(dhakaRest1.Id, "K1", 2, 1),
            RestaurantTable.Create(dhakaRest1.Id, "K2", 2, 1),
            RestaurantTable.Create(dhakaRest1.Id, "K3", 4, 2),
            RestaurantTable.Create(dhakaRest1.Id, "K4", 4, 2),
            RestaurantTable.Create(dhakaRest1.Id, "K5", 6, 4, "Private booth"),
            RestaurantTable.Create(dhakaRest1.Id, "K6", 8, 6, "Family room")
        };
        var tablesDhaka2 = new[]
        {
            RestaurantTable.Create(dhakaRest2.Id, "S1", 2, 1),
            RestaurantTable.Create(dhakaRest2.Id, "S2", 2, 1),
            RestaurantTable.Create(dhakaRest2.Id, "S3", 4, 2),
            RestaurantTable.Create(dhakaRest2.Id, "S4", 4, 2),
            RestaurantTable.Create(dhakaRest2.Id, "S5", 6, 4)
        };
        var tablesDhaka3 = new[]
        {
            RestaurantTable.Create(dhakaRest3.Id, "G1", 2, 1, "Window view"),
            RestaurantTable.Create(dhakaRest3.Id, "G2", 2, 1),
            RestaurantTable.Create(dhakaRest3.Id, "G3", 4, 2),
            RestaurantTable.Create(dhakaRest3.Id, "G4", 4, 2),
            RestaurantTable.Create(dhakaRest3.Id, "G5", 6, 4, "Private dining"),
            RestaurantTable.Create(dhakaRest3.Id, "G6", 8, 6, "Event table")
        };
        var tablesDhaka4 = new[]
        {
            RestaurantTable.Create(dhakaRest4.Id, "F1", 2, 1),
            RestaurantTable.Create(dhakaRest4.Id, "F2", 2, 1),
            RestaurantTable.Create(dhakaRest4.Id, "F3", 4, 2),
            RestaurantTable.Create(dhakaRest4.Id, "F4", 6, 4)
        };
        var tablesDhaka5 = new[]
        {
            RestaurantTable.Create(dhakaRest5.Id, "D1", 2, 1),
            RestaurantTable.Create(dhakaRest5.Id, "D2", 4, 2),
            RestaurantTable.Create(dhakaRest5.Id, "D3", 4, 2),
            RestaurantTable.Create(dhakaRest5.Id, "D4", 6, 4, "Street view")
        };

        await context.Tables.AddRangeAsync([.. tablesRest1, .. tablesRest2, .. tablesRest3,
            .. tablesDhaka1, .. tablesDhaka2, .. tablesDhaka3, .. tablesDhaka4, .. tablesDhaka5]);
        await context.SaveChangesAsync();

        // Sample reservations (future dates)
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var table4 = tablesRest1[2]; // 4-person table at La Bella Italia

        var res1 = Reservation.Create(rest1.Id, table4.Id, customer1.Id,
            futureDate, new TimeOnly(19, 0), 90, 4, "Anniversary dinner");
        await context.Reservations.AddAsync(res1);

        var res2 = Reservation.Create(rest2.Id, tablesRest2[2].Id, customer2.Id,
            futureDate, new TimeOnly(18, 30), 75, 3, null);
        await context.Reservations.AddAsync(res2);
        await context.SaveChangesAsync();

        // Completed reservation for reviews
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var res3 = Reservation.Create(rest1.Id, tablesRest1[0].Id, customer3.Id,
            pastDate, new TimeOnly(20, 0), 90, 2, null);
        res3.Complete();
        await context.Reservations.AddAsync(res3);
        await context.SaveChangesAsync();

        // Review
        var review = Review.Create(rest1.Id, customer3.Id, res3.Id, 5, "Incredible food and atmosphere! Will definitely come back.");
        await context.Reviews.AddAsync(review);
        rest1.UpdateRating(5.0m, 1);
        await context.SaveChangesAsync();
    }
}
