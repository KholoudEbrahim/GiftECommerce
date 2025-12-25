using FluentValidation;

namespace UserProfileService.Features.Commands.RemoveDeliveryAddress
{
    public class RemoveDeliveryAddressCommandValidator : AbstractValidator<RemoveDeliveryAddressCommand>
    {
        public RemoveDeliveryAddressCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.AddressId)
                .NotEmpty().WithMessage("Address ID is required");
        }
    }
}
