using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Restaurants.Queries.GetRestaurantById;

public class GetRestaurantByIdQueryHandler(IRestaurantRepository restaurantRepository)
    : IRequestHandler<GetRestaurantByIdQuery, RestaurantDetailDto>
{
    public async Task<RestaurantDetailDto> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdWithDetailsAsync(request.RestaurantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Restaurant), request.RestaurantId);

        return new RestaurantDetailDto
        {
            Id = restaurant.Id,
            OwnerId = restaurant.OwnerId,
            Name = restaurant.Name,
            Description = restaurant.Description,
            CuisineType = restaurant.CuisineType,
            AddressLine1 = restaurant.AddressLine1,
            AddressLine2 = restaurant.AddressLine2,
            City = restaurant.City,
            State = restaurant.State,
            PostalCode = restaurant.PostalCode,
            Country = restaurant.Country,
            Phone = restaurant.Phone,
            Email = restaurant.Email,
            Website = restaurant.Website,
            AvgRating = restaurant.AvgRating,
            ReviewCount = restaurant.ReviewCount,
            PriceTier = restaurant.PriceTier.ToString(),
            DefaultDurationMinutes = restaurant.DefaultDurationMinutes,
            IsApproved = restaurant.IsApproved,
            IsActive = restaurant.IsActive,
            ImageUrl = restaurant.ImageUrl,
            OpeningHours = restaurant.OpeningHours.OrderBy(h => h.DayOfWeek).Select(h => new OpeningHoursDto
            {
                Id = h.Id,
                DayOfWeek = h.DayOfWeek.ToString(),
                OpenTime = h.OpenTime.ToString("HH:mm"),
                CloseTime = h.CloseTime.ToString("HH:mm"),
                IsClosed = h.IsClosed
            }).ToList(),
            Tables = restaurant.Tables.Where(t => t.IsActive).Select(t => new TableDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                MinCapacity = t.MinCapacity,
                IsActive = t.IsActive,
                Notes = t.Notes
            }).ToList()
        };
    }
}
