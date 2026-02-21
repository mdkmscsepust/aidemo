using ReservationPlatform.Domain.Common;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<Reservation> Reservations { get; private set; } = [];
    public ICollection<Review> Reviews { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    private User() { }

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string? phone,
        UserRole role)
    {
        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Phone = phone?.Trim(),
            Role = role
        };
    }

    public void Update(string firstName, string lastName, string? phone)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public string FullName => $"{FirstName} {LastName}";
}
