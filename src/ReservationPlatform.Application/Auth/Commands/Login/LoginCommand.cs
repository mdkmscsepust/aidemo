using MediatR;
using ReservationPlatform.Application.Auth.DTOs;

namespace ReservationPlatform.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
