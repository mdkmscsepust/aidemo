using MediatR;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Reviews.DTOs;

namespace ReservationPlatform.Application.Reviews.Queries.GetRestaurantReviews;

public record GetRestaurantReviewsQuery(Guid RestaurantId, int Page = 1, int PageSize = 10)
    : IRequest<PaginatedResult<ReviewDto>>;
