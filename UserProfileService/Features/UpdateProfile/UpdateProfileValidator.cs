using FluentValidation;

namespace UserProfileService.Features.UpdateProfile
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
                .Matches(@"^\+?[0-9\s\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must be valid");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow.AddYears(-13)).When(x => x.DateOfBirth.HasValue)
                .WithMessage("User must be at least 13 years old")
                .GreaterThan(DateTime.UtcNow.AddYears(-120)).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Date of birth must be realistic");
        }
    }
}
