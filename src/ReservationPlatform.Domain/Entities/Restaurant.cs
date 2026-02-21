using ReservationPlatform.Domain.Common;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Domain.Entities;

public class Restaurant : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? CuisineType { get; private set; }
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? State { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = "US";
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public decimal AvgRating { get; private set; }
    public int ReviewCount { get; private set; }
    public PriceTier PriceTier { get; private set; } = PriceTier.Moderate;
    public int DefaultDurationMinutes { get; private set; } = 90;
    public bool IsApproved { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? ImageUrl { get; private set; }

    public User? Owner { get; private set; }
    public ICollection<RestaurantTable> Tables { get; private set; } = [];
    public ICollection<OpeningHours> OpeningHours { get; private set; } = [];
    public ICollection<Review> Reviews { get; private set; } = [];

    private Restaurant() { }

    public static Restaurant Create(
        Guid ownerId,
        string name,
        string? description,
        string? cuisineType,
        string addressLine1,
        string? addressLine2,
        string city,
        string? state,
        string postalCode,
        string country,
        string? phone,
        string? email,
        PriceTier priceTier,
        int defaultDurationMinutes = 90)
    {
        return new Restaurant
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Description = description?.Trim(),
            CuisineType = cuisineType?.Trim(),
            AddressLine1 = addressLine1.Trim(),
            AddressLine2 = addressLine2?.Trim(),
            City = city.Trim(),
            State = state?.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            PriceTier = priceTier,
            DefaultDurationMinutes = defaultDurationMinutes > 0 ? defaultDurationMinutes : 90
        };
    }

    public void Update(
        string name,
        string? description,
        string? cuisineType,
        string addressLine1,
        string? addressLine2,
        string city,
        string? state,
        string postalCode,
        string? phone,
        string? email,
        string? website,
        PriceTier priceTier,
        int defaultDurationMinutes,
        string? imageUrl)
    {
        Name = name.Trim();
        Description = description?.Trim();
        CuisineType = cuisineType?.Trim();
        AddressLine1 = addressLine1.Trim();
        AddressLine2 = addressLine2?.Trim();
        City = city.Trim();
        State = state?.Trim();
        PostalCode = postalCode.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim().ToLowerInvariant();
        Website = website?.Trim();
        PriceTier = priceTier;
        DefaultDurationMinutes = defaultDurationMinutes > 0 ? defaultDurationMinutes : 90;
        ImageUrl = imageUrl?.Trim();
    }

    public void Approve() => IsApproved = true;
    public void Reject() => IsApproved = false;
    public void Deactivate() => IsActive = false;

    public void UpdateRating(decimal avgRating, int reviewCount)
    {
        AvgRating = avgRating;
        ReviewCount = reviewCount;
    }
}
