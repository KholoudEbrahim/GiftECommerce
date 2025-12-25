using FluentValidation;

namespace UserProfileService.Features.Queries.ListDeliveryAddresses
{
    public class GetUserAddressesQueryValidator : AbstractValidator<GetUserAddressesQuery>
    {
        public GetUserAddressesQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
        }
    }
}
