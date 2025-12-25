using FluentValidation;

namespace UserProfileService.Features.Queries.GetProfile
{
    public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
    {
        public GetUserProfileQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("User ID cannot be empty");
        }
    }
}
