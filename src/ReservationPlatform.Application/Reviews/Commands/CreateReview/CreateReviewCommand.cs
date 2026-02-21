using MediatR;
using ReservationPlatform.Application.Reviews.DTOs;

namespace ReservationPlatform.Application.Reviews.Commands.CreateReview;

public record CreateReviewCommand(
    Guid RestaurantId,
    Guid CustomerId,
    Guid ReservationId,
    int Rating,
    string? Comment
) : IRequest<ReviewDto>;
