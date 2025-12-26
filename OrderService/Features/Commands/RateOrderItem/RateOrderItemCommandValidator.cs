using FluentValidation;

namespace OrderService.Features.Commands.RateOrderItem
{
    public class RateOrderItemCommandValidator : AbstractValidator<RateOrderItemCommand>
    {
        public RateOrderItemCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.OrderItemId)
                .GreaterThan(0)
                .WithMessage("Order item ID must be greater than 0");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .When(x => x.Rating <= 3)
                .WithMessage("Comment is required for ratings 3 or below")
                .MaximumLength(1000)
                .WithMessage("Comment cannot exceed 1000 characters");

            RuleFor(x => x.Comment)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrEmpty(x.Comment))
                .WithMessage("Comment cannot exceed 1000 characters");
        }
    }
}
