using FluentValidation;
using OrderService.Data;

namespace OrderService.Features.Commands.VerifyCashPayment
{
    public class VerifyCashPaymentCommandValidator : AbstractValidator<VerifyCashPaymentCommand>
    {
        public VerifyCashPaymentCommandValidator()
        {
            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .WithMessage("Order number is required")
                .MaximumLength(50)
                .WithMessage("Order number cannot exceed 50 characters");

            RuleFor(x => x.VerifiedBy)
                .NotEmpty()
                .WithMessage("Verifier ID is required");

            RuleFor(x => x.TransactionId)
                .MaximumLength(100)
                .WithMessage("Transaction ID cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.TransactionId));

         
        }
    }
}