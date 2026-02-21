using MediatR;
using ReservationPlatform.Application.Auth.DTOs;

namespace ReservationPlatform.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<AuthResponseDto>;
