using MediatR;
using ReservationPlatform.Application.Common.Exceptions;
using ReservationPlatform.Application.Reviews.DTOs;
using ReservationPlatform.Domain.Entities;
using ReservationPlatform.Domain.Enums;
using ReservationPlatform.Domain.Interfaces;
using ReservationPlatform.Domain.Interfaces.Repositories;

namespace ReservationPlatform.Application.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler(
    IReviewRepository reviewRepository,
    IReservationRepository reservationRepository,
    IRestaurantRepository restaurantRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.CustomerId != request.CustomerId)
            throw new ForbiddenException("You can only review your own reservations.");

        if (reservation.RestaurantId != request.RestaurantId)
            throw new ConflictException("Reservation does not belong to this restaurant.");

        if (reservation.Status != ReservationStatus.Completed)
            throw new ConflictException("You can only review completed reservations.");

        var existingReview = await reviewRepository.GetByReservationIdAsync(request.ReservationId, cancellationToken);
        if (existingReview != null)
            throw new ConflictException("You have already reviewed this reservation.");

        var review = Review.Create(request.RestaurantId, request.CustomerId, request.ReservationId, request.Rating, request.Comment);
        await reviewRepository.AddAsync(review, cancellationToken);

        // Update restaurant average rating
        var (avgRating, count) = await reviewRepository.GetRatingStatsAsync(request.RestaurantId, cancellationToken);
        // Note: this is approximate; the new review isn't saved yet. We'll compute after save.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Recalculate after save
        var (newAvg, newCount) = await reviewRepository.GetRatingStatsAsync(request.RestaurantId, cancellationToken);
        var restaurant = await restaurantRepository.GetByIdAsync(request.RestaurantId, cancellationToken);
        restaurant?.UpdateRating(newAvg, newCount);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            RestaurantId = review.RestaurantId,
            CustomerId = review.CustomerId,
            CustomerName = string.Empty,
            ReservationId = review.ReservationId,
            Rating = review.Rating,
            Comment = review.Comment,
            IsPublished = review.IsPublished,
            CreatedAt = review.CreatedAt
        };
    }
}
