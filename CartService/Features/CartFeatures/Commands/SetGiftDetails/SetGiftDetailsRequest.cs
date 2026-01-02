using System.ComponentModel.DataAnnotations;

namespace CartService.Features.CartFeatures.Commands.SetGiftDetails
{
    public record SetGiftDetailsRequest
    {
        [Required]
        public string RecipientName { get; init; } = default!;

        [Required]
        [Phone]
        public string RecipientPhone { get; init; } = default!;

        [MaxLength(500)]
        public string? GiftMessage { get; init; }

        [FutureDate(ErrorMessage = "Delivery date must be in the future")]
        public DateTime? DeliveryDate { get; init; }

        public bool GiftWrap { get; init; } = false;
    }
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date.Date <= DateTime.Today)
                    return new ValidationResult(ErrorMessage ?? "Date must be in the future");
            }

            return ValidationResult.Success;
        }
    }
}