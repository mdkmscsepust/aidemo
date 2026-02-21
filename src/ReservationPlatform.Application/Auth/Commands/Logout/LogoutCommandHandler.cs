using MediatR;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (token != null && token.IsActive)
        {
            token.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
