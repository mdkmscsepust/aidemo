using ReservationPlatform.Domain.Common;

namespace ReservationPlatform.Domain.Entities;

public class OpeningHours : BaseEntity
{
    public Guid RestaurantId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    public Restaurant? Restaurant { get; private set; }

    private OpeningHours() { }

    public static OpeningHours Create(
        Guid restaurantId,
        DayOfWeek dayOfWeek,
        TimeOnly openTime,
        TimeOnly closeTime,
        bool isClosed = false)
    {
        return new OpeningHours
        {
            RestaurantId = restaurantId,
            DayOfWeek = dayOfWeek,
            OpenTime = openTime,
            CloseTime = closeTime,
            IsClosed = isClosed
        };
    }

    public void Update(TimeOnly openTime, TimeOnly closeTime, bool isClosed)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }
}
