using MediatR;

namespace ReservationPlatform.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
