namespace ReservationPlatform.Application.Tables.DTOs;

public class CreateTableDto
{
    public Guid RestaurantId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int MinCapacity { get; set; } = 1;
    public string? Notes { get; set; }
}
