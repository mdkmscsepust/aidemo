namespace ReservationPlatform.Application.OpeningHours.DTOs;

public class OpeningHoursInputDto
{
    public string DayOfWeek { get; set; } = string.Empty;  // "Monday", "Tuesday", etc.
    public string OpenTime { get; set; } = "09:00";         // HH:mm
    public string CloseTime { get; set; } = "22:00";        // HH:mm
    public bool IsClosed { get; set; }
}
