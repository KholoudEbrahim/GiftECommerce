using FluentValidation;

namespace IdentityService.Features.Commands.PasswordReset.VerifyResetCode
{
    public class VerifyResetCodeValidator : AbstractValidator<VerifyResetCodeCommand>
    {
        public VerifyResetCodeValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(6).WithMessage("Code must be 6 digits")
                .Matches(@"^\d+$").WithMessage("Code must contain only digits");
        }
    }
}
