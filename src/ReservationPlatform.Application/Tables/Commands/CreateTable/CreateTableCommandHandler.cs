using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Tables.Commands.CreateTable;

public class CreateTableCommandHandler(
    IRestaurantRepository restaurantRepository,
    ITableRepository tableRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateTableCommand, TableDto>
{
    public async Task<TableDto> Handle(CreateTableCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Restaurant), request.RestaurantId);

        if (request.RequestingUserRole != "Admin" && restaurant.OwnerId != request.RequestingUserId)
            throw new ForbiddenException("You do not own this restaurant.");

        var table = RestaurantTable.Create(
            request.RestaurantId, request.TableNumber, request.Capacity, request.MinCapacity, request.Notes);

        await tableRepository.AddAsync(table, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            MinCapacity = table.MinCapacity,
            IsActive = table.IsActive,
            Notes = table.Notes
        };
    }
}
