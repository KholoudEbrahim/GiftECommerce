using FluentValidation;

namespace CartService.Features.CartFeatures.Commands.UpdateItemQuantity
{
    public class UpdateItemQuantityValidator : AbstractValidator<UpdateItemQuantity.UpdateCartItemQuantityCommand>
    {
        public UpdateItemQuantityValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity must be 0 or greater")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity cannot exceed 100 items");

            RuleFor(x => x)
                .Must(x => x.UserId.HasValue || !string.IsNullOrEmpty(x.AnonymousId))
                .WithMessage("Either UserId or AnonymousId must be provided");
        }
    }
}
