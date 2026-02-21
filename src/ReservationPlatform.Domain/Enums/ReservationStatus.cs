namespace ReservationPlatform.Domain.Enums;

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Completed = 2,
    CancelledByCustomer = 3,
    CancelledByRestaurant = 4,
    NoShow = 5
}
