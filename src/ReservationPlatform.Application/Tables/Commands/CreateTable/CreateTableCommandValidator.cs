using FluentValidation;

namespace ReservationPlatform.Application.Tables.Commands.CreateTable;

public class CreateTableCommandValidator : AbstractValidator<CreateTableCommand>
{
    public CreateTableCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.TableNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 50);
        RuleFor(x => x.MinCapacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
