using FluentValidation;
using OrderService.Data;

namespace OrderService.Features.Commands.VerifyCashPayment
{
    public class VerifyCashPaymentCommandValidator : AbstractValidator<VerifyCashPaymentCommand>
    {
        private readonly IOrderRepository _orderRepository;

        public VerifyCashPaymentCommandValidator(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;

            RuleFor(x => x.OrderNumber)
                .NotEmpty().WithMessage("Order number is required")
                .MaximumLength(50).WithMessage("Order number cannot exceed 50 characters");

            RuleFor(x => x.VerifiedBy)
                .NotEmpty().WithMessage("Verifier ID is required");

            RuleFor(x => x.TransactionId)
                .MaximumLength(100).WithMessage("Transaction ID cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.TransactionId));

            RuleFor(x => x.OrderNumber)
                .MustAsync(async (orderNumber, cancellation) =>
                {
                    var order = await _orderRepository.GetOrderWithDetailsAsync(orderNumber, cancellation);
                    return order != null;
                })
                .WithMessage("Order not found")
                .DependentRules(() =>
                {
                    RuleFor(x => x.OrderNumber)
                        .MustAsync(async (orderNumber, cancellation) =>
                        {
                            var order = await _orderRepository.GetOrderWithDetailsAsync(orderNumber, cancellation);
                            return order?.Payment?.Method == Models.enums.PaymentMethod.CashOnDelivery;
                        })
                        .WithMessage("Only cash on delivery orders can be verified");

                    RuleFor(x => x.OrderNumber)
                        .MustAsync(async (orderNumber, cancellation) =>
                        {
                            var order = await _orderRepository.GetOrderWithDetailsAsync(orderNumber, cancellation);
                            return order?.Payment?.Status == Models.enums.PaymentStatus.AwaitingCashPayment;
                        })
                        .WithMessage("Payment is not in awaiting cash payment state");
                });
        }
    }
}