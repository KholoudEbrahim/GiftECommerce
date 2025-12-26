using FluentValidation;

namespace OrderService.Features.Commands.ReOrder
{
    public class ReOrderCommandValidator : AbstractValidator<ReOrderCommand>
    {
        public ReOrderCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("Order number is required");

            When(x => x.ModifiedItems != null, () =>
            {
                RuleForEach(x => x.ModifiedItems!)
                    .ChildRules(item =>
                    {
                        item.RuleFor(i => i.ProductId)
                            .GreaterThan(0)
                            .WithMessage("Product ID must be greater than 0");

                        item.RuleFor(i => i.NewQuantity)
                            .GreaterThan(0)
                            .When(i => i.NewQuantity.HasValue)
                            .WithMessage("Quantity must be greater than 0");
                    });
            });
        }
    }
}
