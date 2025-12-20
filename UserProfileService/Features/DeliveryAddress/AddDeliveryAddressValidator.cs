using FluentValidation;

namespace UserProfileService.Features.DeliveryAddress
{
    public class AddDeliveryAddressValidator : AbstractValidator<AddDeliveryAddressCommand>
    {
        public AddDeliveryAddressValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Alias)
                .NotEmpty().WithMessage("Address alias is required")
                .MaximumLength(50).WithMessage("Alias must not exceed 50 characters");

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required")
                .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters");

            RuleFor(x => x.Governorate)
                .NotEmpty().WithMessage("Governorate is required")
                .MaximumLength(100).WithMessage("Governorate must not exceed 100 characters");

            RuleFor(x => x.Building)
                .NotEmpty().WithMessage("Building is required")
                .MaximumLength(50).WithMessage("Building must not exceed 50 characters");

            RuleFor(x => x.Floor)
                .MaximumLength(20).WithMessage("Floor must not exceed 20 characters");

            RuleFor(x => x.Apartment)
                .MaximumLength(20).WithMessage("Apartment must not exceed 20 characters");
        }
    }
}
