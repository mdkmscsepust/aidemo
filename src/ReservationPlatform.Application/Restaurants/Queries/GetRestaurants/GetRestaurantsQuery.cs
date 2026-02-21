using MediatR;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Restaurants.DTOs;

namespace ReservationPlatform.Application.Restaurants.Queries.GetRestaurants;

public record GetRestaurantsQuery(
    string? Search,
    string? City,
    string? CuisineType,
    int? PriceTierMin,
    int? PriceTierMax,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "rating"
) : IRequest<PaginatedResult<RestaurantListDto>>;
