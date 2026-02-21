using ReservationPlatform.Domain.Common;

namespace ReservationPlatform.Domain.Entities;

public class Review : BaseEntity
{
    public Guid RestaurantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ReservationId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsPublished { get; private set; } = true;

    public Restaurant? Restaurant { get; private set; }
    public User? Customer { get; private set; }
    public Reservation? Reservation { get; private set; }

    private Review() { }

    public static Review Create(
        Guid restaurantId,
        Guid customerId,
        Guid reservationId,
        int rating,
        string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");

        return new Review
        {
            RestaurantId = restaurantId,
            CustomerId = customerId,
            ReservationId = reservationId,
            Rating = rating,
            Comment = comment?.Trim()
        };
    }

    public void Unpublish() => IsPublished = false;
    public void Publish() => IsPublished = true;
}
