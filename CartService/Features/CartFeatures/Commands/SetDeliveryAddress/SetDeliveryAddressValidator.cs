using FluentValidation;

namespace CartService.Features.CartFeatures.Commands.SetDeliveryAddress
{
    public class SetDeliveryAddressValidator : AbstractValidator<SetDeliveryAddress.SetDeliveryAddressCommand>
    {
        public SetDeliveryAddressValidator()
        {
            RuleFor(x => x.AddressId)
                .NotEmpty()
                .WithMessage("Address ID is required");

            RuleFor(x => x)
                .Must(x => x.UserId.HasValue || !string.IsNullOrEmpty(x.AnonymousId))
                .WithMessage("Either UserId or AnonymousId must be provided");

            When(x => x.UserId.HasValue, () =>
            {
                RuleFor(x => x.UserId)
                    .NotEmpty()
                    .WithMessage("UserId is required when setting delivery address");
            });
        }
    }
}
