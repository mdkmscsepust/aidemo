using MediatR;
using ReservationPlatform.Application.Auth.DTOs;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;
using DomainUser = ReservationPlatform.Domain.Entities.User;
using DomainRefreshToken = ReservationPlatform.Domain.Entities.RefreshToken;

namespace ReservationPlatform.Application.Auth.Commands.Register;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    string GenerateAccessToken(DomainUser user);
    (string Token, DateTime ExpiresAt) GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
}

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = DomainUser.Create(request.Email, passwordHash, request.FirstName, request.LastName, request.Phone, request.Role);
        await userRepository.AddAsync(user, cancellationToken);

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
