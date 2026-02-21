using MediatR;
using ReservationPlatform.Application.Auth.Commands.Register;
using ReservationPlatform.Application.Auth.DTOs;
using ReservationPlatform.Application.Common.Exceptions;
using DomainRefreshToken = ReservationPlatform.Domain.Entities.RefreshToken;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new ForbiddenException("Invalid refresh token.");

        if (existingToken.IsRevoked)
        {
            // Token reuse detected — revoke entire family
            await refreshTokenRepository.RevokeAllByUserAsync(existingToken.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException("Token reuse detected. All sessions have been revoked.");
        }

        if (!existingToken.IsActive)
            throw new ForbiddenException("Refresh token has expired.");

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new ForbiddenException("User not found.");

        if (!user.IsActive)
            throw new ForbiddenException("Account is deactivated.");

        var (newTokenValue, newExpiry) = tokenService.GenerateRefreshToken();
        var newRefreshToken = DomainRefreshToken.Create(user.Id, newTokenValue, newExpiry);
        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // Mark old token as replaced
        existingToken.Revoke(newRefreshToken.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = tokenService.GenerateAccessToken(user),
            RefreshToken = newTokenValue,
            ExpiresAt = tokenService.GetAccessTokenExpiry(),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Role = user.Role.ToString()
            }
        };
    }
}
