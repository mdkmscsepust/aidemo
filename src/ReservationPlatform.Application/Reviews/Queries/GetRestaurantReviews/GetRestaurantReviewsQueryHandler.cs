using MediatR;
using ReservationPlatform.Application.Common.Models;
using ReservationPlatform.Application.Reviews.DTOs;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reviews.Queries.GetRestaurantReviews;

public class GetRestaurantReviewsQueryHandler(IReviewRepository reviewRepository)
    : IRequestHandler<GetRestaurantReviewsQuery, PaginatedResult<ReviewDto>>
{
    public async Task<PaginatedResult<ReviewDto>> Handle(GetRestaurantReviewsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await reviewRepository.GetByRestaurantAsync(
            request.RestaurantId, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(r => new ReviewDto
        {
            Id = r.Id,
            RestaurantId = r.RestaurantId,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : string.Empty,
            ReservationId = r.ReservationId,
            Rating = r.Rating,
            Comment = r.Comment,
            IsPublished = r.IsPublished,
            CreatedAt = r.CreatedAt
        });

        return PaginatedResult<ReviewDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
