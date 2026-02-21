using MediatR;
using ReservationPlatform.Application.Restaurants.DTOs;

namespace ReservationPlatform.Application.Tables.Commands.CreateTable;

public record CreateTableCommand(
    Guid RestaurantId,
    Guid RequestingUserId,
    string RequestingUserRole,
    string TableNumber,
    int Capacity,
    int MinCapacity,
    string? Notes
) : IRequest<TableDto>;
