using FluentValidation;

namespace IdentityService.Features.Commands.PasswordReset.RequestPasswordReset
{
    public class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetCommand>
    {
        public RequestPasswordResetValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }

}
