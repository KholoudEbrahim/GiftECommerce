using FluentValidation;

namespace OccasionService.Features.UpdateOccasion
{
    public class UpdateOccasionValidator : AbstractValidator<UpdateOccasionCommand>
    {
        public UpdateOccasionValidator()
        {
            RuleFor(x => x.Id)
           .NotEmpty()
           .WithMessage("Occasion ID is required");

            RuleFor(x => x.Name)
           .NotEmpty()
           .WithMessage("Occasion name is required");

            RuleFor(x => x.Name)
            .Length(3, 100)
            .WithMessage("Occasion name must be between 3 and 100 characters");

            RuleFor(x => x.ImageUrl)
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Invalid image URL format");
        }
    }
}
