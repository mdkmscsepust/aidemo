using ReservationPlatform.Domain.Common;

namespace ReservationPlatform.Domain.Entities;

public class RestaurantTable : BaseEntity
{
    public Guid RestaurantId { get; private set; }
    public string TableNumber { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public int MinCapacity { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    public Restaurant? Restaurant { get; private set; }
    public ICollection<Reservation> Reservations { get; private set; } = [];

    private RestaurantTable() { }

    public static RestaurantTable Create(
        Guid restaurantId,
        string tableNumber,
        int capacity,
        int minCapacity = 1,
        string? notes = null)
    {
        if (capacity <= 0) throw new ArgumentException("Capacity must be positive.", nameof(capacity));
        if (minCapacity < 1) minCapacity = 1;
        if (minCapacity > capacity) minCapacity = 1;

        return new RestaurantTable
        {
            RestaurantId = restaurantId,
            TableNumber = tableNumber.Trim(),
            Capacity = capacity,
            MinCapacity = minCapacity,
            Notes = notes?.Trim()
        };
    }

    public void Update(string tableNumber, int capacity, int minCapacity, string? notes)
    {
        TableNumber = tableNumber.Trim();
        Capacity = capacity > 0 ? capacity : Capacity;
        MinCapacity = minCapacity >= 1 && minCapacity <= capacity ? minCapacity : 1;
        Notes = notes?.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
