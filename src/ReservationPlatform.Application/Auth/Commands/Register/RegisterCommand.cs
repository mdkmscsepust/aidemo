using MediatR;
using ReservationPlatform.Application.Auth.DTOs;
using ReservationPlatform.Domain.Enums;

namespace ReservationPlatform.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    UserRole Role = UserRole.Customer
) : IRequest<AuthResponseDto>;
