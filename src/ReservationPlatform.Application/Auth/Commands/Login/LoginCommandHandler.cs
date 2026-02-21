using MediatR;
using ReservationPlatform.Application.Auth.Commands.Register;
using ReservationPlatform.Application.Auth.DTOs;
using ReservationPlatform.Application.Common.Exceptions;
using DomainRefreshToken = ReservationPlatform.Domain.Entities.RefreshToken;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork
) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new ForbiddenException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenException("Account is deactivated.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new ForbiddenException("Invalid email or password.");

        var (refreshTokenValue, refreshExpiry) = tokenService.GenerateRefreshToken();
        var refreshToken = DomainRefreshToken.Create(user.Id, refreshTokenValue, refreshExpiry);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = tokenService.GenerateAccessToken(user),
            RefreshToken = refreshTokenValue,
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
