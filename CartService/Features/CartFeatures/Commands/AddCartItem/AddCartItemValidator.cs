using FluentValidation;

namespace CartService.Features.CartFeatures.Commands.AddCartItem
{
    public class AddCartItemValidator : AbstractValidator<AddCartItemCommand>
    {
        public AddCartItemValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity cannot exceed 100 items");

            RuleFor(x => x)
                .Must(x => x.UserId.HasValue || !string.IsNullOrEmpty(x.AnonymousId))
                .WithMessage("Either UserId or AnonymousId must be provided");

            When(x => x.UserId.HasValue, () =>
            {
                RuleFor(x => x.UserId)
                    .NotEmpty()
                    .WithMessage("UserId must not be empty");
            });

            When(x => !string.IsNullOrEmpty(x.AnonymousId), () =>
            {
                RuleFor(x => x.AnonymousId)
                    .MinimumLength(10)
                    .MaximumLength(100)
                    .WithMessage("AnonymousId must be between 10 and 100 characters");
            });
        }
    }
}
