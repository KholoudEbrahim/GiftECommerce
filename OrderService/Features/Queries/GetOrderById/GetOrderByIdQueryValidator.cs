using FluentValidation;

namespace OrderService.Features.Queries.GetOrderById
{
    public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.OrderNumber)
                .NotEmpty().WithMessage("Order number is required")
                .MaximumLength(50).WithMessage("Order number is too long");
        }
    }
}
