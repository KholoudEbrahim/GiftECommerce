using FluentValidation;

namespace IdentityService.Features.Commands.Login
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");


            When(x => !string.IsNullOrEmpty(x.RequiredRole), () =>
            {
                RuleFor(x => x.RequiredRole)
                    .Must(role => role == "Admin" || role == "User" || role == "Moderator")
                    .WithMessage("Invalid role specified");
            });
        }
    }
}
