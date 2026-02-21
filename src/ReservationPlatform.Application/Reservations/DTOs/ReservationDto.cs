namespace ReservationPlatform.Application.Reservations.DTOs;

public class ReservationDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public Guid TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int PartySize { get; set; }
    public string ReservationDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SpecialRequests { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AvailableSlotDto
{
    public string SlotTime { get; set; } = string.Empty;
    public Guid TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int TableCapacity { get; set; }
}
