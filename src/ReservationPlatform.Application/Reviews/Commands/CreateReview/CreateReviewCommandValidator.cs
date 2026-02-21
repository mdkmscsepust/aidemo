using FluentValidation;

namespace ReservationPlatform.Application.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment).MaximumLength(2000).When(x => x.Comment != null);
    }
}
