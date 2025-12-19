using FluentValidation;

namespace CartService.Features.CartFeatures.Commands.RemoveCartItem
{
    public class RemoveItemValidator : AbstractValidator<RemoveCartItemCommand>
    {
        public RemoveItemValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x)
                .Must(x => x.UserId.HasValue || !string.IsNullOrEmpty(x.AnonymousId))
                .WithMessage("Either UserId or AnonymousId must be provided");
        }
    }
}
