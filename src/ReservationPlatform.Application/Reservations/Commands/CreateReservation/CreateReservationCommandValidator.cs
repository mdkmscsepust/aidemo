using FluentValidation;

namespace ReservationPlatform.Application.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.TableId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.PartySize).InclusiveBetween(1, 20);
        RuleFor(x => x.SpecialRequests).MaximumLength(1000).When(x => x.SpecialRequests != null);

        RuleFor(x => x.ReservationDate)
            .NotEmpty()
            .Must(d => DateOnly.TryParseExact(d, "yyyy-MM-dd", out _))
            .WithMessage("ReservationDate must be in yyyy-MM-dd format.")
            .Must(d =>
            {
                if (!DateOnly.TryParseExact(d, "yyyy-MM-dd", out var date)) return false;
                return date >= DateOnly.FromDateTime(DateTime.UtcNow);
            })
            .WithMessage("Reservation date must be today or in the future.");

        RuleFor(x => x.SlotTime)
            .NotEmpty()
            .Must(t => TimeOnly.TryParseExact(t, "HH:mm", out _))
            .WithMessage("SlotTime must be in HH:mm format.");
    }
}
