using MediatR;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Reservations.DTOs;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryHandler(IReservationRepository reservationRepository)
    : IRequestHandler<GetMyReservationsQuery, PaginatedResult<ReservationDto>>
{
    public async Task<PaginatedResult<ReservationDto>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await reservationRepository.GetByCustomerAsync(
            request.CustomerId, request.Status, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new ReservationDto
        {
            Id = r.Id,
            RestaurantId = r.RestaurantId,
            RestaurantName = r.Restaurant?.Name ?? string.Empty,
            TableId = r.TableId,
            TableNumber = r.Table?.TableNumber ?? string.Empty,
            TableCapacity = r.Table?.Capacity ?? 0,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : string.Empty,
            PartySize = r.PartySize,
            ReservationDate = r.ReservationDate.ToString("yyyy-MM-dd"),
            StartTime = r.StartTime.ToString("HH:mm"),
            EndTime = r.EndTime.ToString("HH:mm"),
            DurationMinutes = r.DurationMinutes,
            Status = r.Status.ToString(),
            SpecialRequests = r.SpecialRequests,
            ConfirmationCode = r.ConfirmationCode,
            Notes = r.Notes,
            CancelledAt = r.CancelledAt,
            CancellationReason = r.CancellationReason,
            CreatedAt = r.CreatedAt
        });

        return PaginatedResult<ReservationDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
