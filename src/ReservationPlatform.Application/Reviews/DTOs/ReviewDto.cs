namespace ReservationPlatform.Application.Reviews.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid ReservationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
