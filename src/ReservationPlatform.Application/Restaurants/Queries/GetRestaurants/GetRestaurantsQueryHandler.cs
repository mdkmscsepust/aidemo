using MediatR;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Restaurants.DTOs;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Restaurants.Queries.GetRestaurants;

public class GetRestaurantsQueryHandler(IRestaurantRepository restaurantRepository)
    : IRequestHandler<GetRestaurantsQuery, PaginatedResult<RestaurantListDto>>
{
    public async Task<PaginatedResult<RestaurantListDto>> Handle(GetRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await restaurantRepository.SearchAsync(
            request.Search, request.City, request.CuisineType,
            request.PriceTierMin, request.PriceTierMax,
            request.Page, request.PageSize, request.SortBy, cancellationToken);

        var dtos = items.Select(r => new RestaurantListDto
        {
            Id = r.Id,
            Name = r.Name,
            CuisineType = r.CuisineType,
            City = r.City,
            AddressLine1 = r.AddressLine1,
            AvgRating = r.AvgRating,
            ReviewCount = r.ReviewCount,
            PriceTier = r.PriceTier.ToString(),
            ImageUrl = r.ImageUrl,
            IsApproved = r.IsApproved
        });

        return PaginatedResult<RestaurantListDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
