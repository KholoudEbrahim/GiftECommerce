using FluentValidation;

namespace OrderService.Features.Queries.TrackOrder
{
    public class TrackOrderQueryValidator : AbstractValidator<TrackOrderQuery>
    {
        public TrackOrderQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.OrderNumber)
                .NotEmpty().WithMessage("Order number is required")
                .MaximumLength(50).WithMessage("Order number is too long");
        }
    }
}
