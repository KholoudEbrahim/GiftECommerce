using FluentValidation;

namespace OrderService.Features.Commands.PlaceOrder
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.DeliveryAddressId)
                .NotEmpty()
                .WithMessage("Delivery address is required");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithMessage("Invalid payment method");

            When(x => x.CartId.HasValue, () =>
            {
                RuleFor(x => x.CartId)
                    .NotEmpty()
                    .WithMessage("Cart ID must be valid when provided");
            });

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Notes))
                .WithMessage("Notes cannot exceed 500 characters");
        }
    }
}
