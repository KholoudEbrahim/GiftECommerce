using FluentValidation;

namespace CartService.Features.CartFeatures.Queries.GetCart
{
    public class GetCartValidator : AbstractValidator<GetCart.GetCartQuery>
    {
        public GetCartValidator()
        {
            RuleFor(x => x)
                .Must(x => x.UserId.HasValue || !string.IsNullOrEmpty(x.AnonymousId))
                .WithMessage("Either UserId or AnonymousId must be provided");
        }

    }
}
