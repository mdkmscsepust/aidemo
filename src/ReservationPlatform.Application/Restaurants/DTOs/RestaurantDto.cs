using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Application.Restaurants.DTOs;

public class RestaurantListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CuisineType { get; set; }
    public string City { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public string PriceTier { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsApproved { get; set; }
}

public class RestaurantDetailDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CuisineType { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public string PriceTier { get; set; } = string.Empty;
    public int DefaultDurationMinutes { get; set; }
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public List<OpeningHoursDto> OpeningHours { get; set; } = [];
    public List<TableDto> Tables { get; set; } = [];
}

public class OpeningHoursDto
{
    public Guid Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
}

public class TableDto
{
    public Guid Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int MinCapacity { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
