using ReservationPlatform.Domain.Common;
using ReservationPlatform.Domain.Enums;
using System.Security.Cryptography;

namespace ReservationPlatform.Domain.Entities;

public class Reservation : BaseEntity
{
    public Guid RestaurantId { get; private set; }
    public Guid TableId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int PartySize { get; private set; }
    public DateOnly ReservationDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Confirmed;
    public string? SpecialRequests { get; private set; }
    public string ConfirmationCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public Restaurant? Restaurant { get; private set; }
    public RestaurantTable? Table { get; private set; }
    public User? Customer { get; private set; }
    public Review? Review { get; private set; }

    private Reservation() { }

    public static Reservation Create(
        Guid restaurantId,
        Guid tableId,
        Guid customerId,
        DateOnly reservationDate,
        TimeOnly startTime,
        int durationMinutes,
        int partySize,
        string? specialRequests)
    {
        if (partySize <= 0) throw new ArgumentException("Party size must be positive.", nameof(partySize));
        if (durationMinutes <= 0) throw new ArgumentException("Duration must be positive.", nameof(durationMinutes));

        return new Reservation
        {
            RestaurantId = restaurantId,
            TableId = tableId,
            CustomerId = customerId,
            ReservationDate = reservationDate,
            StartTime = startTime,
            EndTime = startTime.AddMinutes(durationMinutes),
            DurationMinutes = durationMinutes,
            PartySize = partySize,
            SpecialRequests = specialRequests?.Trim(),
            ConfirmationCode = GenerateConfirmationCode(),
            Status = ReservationStatus.Confirmed
        };
    }

    public void Cancel(string? reason, bool byRestaurant = false)
    {
        if (Status is ReservationStatus.Completed
            or ReservationStatus.CancelledByCustomer
            or ReservationStatus.CancelledByRestaurant)
        {
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{Status}'.");
        }

        Status = byRestaurant
            ? ReservationStatus.CancelledByRestaurant
            : ReservationStatus.CancelledByCustomer;

        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason?.Trim();
    }

    public void Complete()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed reservations can be completed.");
        Status = ReservationStatus.Completed;
    }

    public void MarkNoShow()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed reservations can be marked as no-show.");
        Status = ReservationStatus.NoShow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    public bool CanBeCancelledByCustomer(int cancellationWindowHours = 2)
    {
        var reservationDateTime = ReservationDate.ToDateTime(StartTime, DateTimeKind.Utc);
        return DateTime.UtcNow.AddHours(cancellationWindowHours) < reservationDateTime;
    }

    private static string GenerateConfirmationCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
