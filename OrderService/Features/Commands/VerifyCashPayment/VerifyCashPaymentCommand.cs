using FluentValidation;
using MediatR;
using OrderService.Data;
using OrderService.Events.Publisher;
using OrderService.Models.enums;

namespace OrderService.Features.Commands.VerifyCashPayment
{
    public record VerifyCashPaymentCommand(
         string OrderNumber,
         Guid VerifiedBy, 
         string? TransactionId = null) : IRequest<VerifyCashPaymentResultDto>;

    public class VerifyCashPaymentCommandHandler
        : IRequestHandler<VerifyCashPaymentCommand, VerifyCashPaymentResultDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<VerifyCashPaymentCommandHandler> _logger;
        private readonly IValidator<VerifyCashPaymentCommand> _validator;

        public VerifyCashPaymentCommandHandler(
       IOrderRepository orderRepository,
       IEventPublisher eventPublisher,
       ILogger<VerifyCashPaymentCommandHandler> logger,
       IValidator<VerifyCashPaymentCommand> validator) 
        {
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _validator = validator; 
        }

        public async Task<VerifyCashPaymentResultDto> Handle(
            VerifyCashPaymentCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
             
                var order = await _orderRepository.GetOrderWithDetailsAsync(
                    request.OrderNumber,
                    cancellationToken);

                if (order == null)
                    throw new KeyNotFoundException($"Order {request.OrderNumber} not found");

                if (order.Payment == null)
                    throw new InvalidOperationException("Order has no payment");

                if (order.Payment.Method != PaymentMethod.CashOnDelivery)
                    throw new InvalidOperationException("Only cash on delivery orders can be verified");

                if (order.Payment.Status != PaymentStatus.AwaitingCashPayment)
                    throw new InvalidOperationException($"Payment is in {order.Payment.Status} state, cannot verify");

              
                var oldPaymentStatus = order.Payment.Status;
                order.Payment.MarkAsCashPaymentVerified(request.TransactionId);
                order.UpdatePaymentStatus(PaymentStatus.CashPaymentVerified);

                if (order.Status == OrderStatus.Pending)
                {
                    var oldOrderStatus = order.Status;
                    order.UpdateStatus(OrderStatus.Confirmed);

              
                    await _eventPublisher.PublishOrderStatusUpdatedAsync(
                        order,
                        oldOrderStatus.ToString(),
                        cancellationToken);
                }

               
                await _orderRepository.UpdatePaymentAsync(order.Payment);
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync(cancellationToken);

             
                await _eventPublisher.PublishPaymentCompletedAsync(order, order.Payment, cancellationToken);

                _logger.LogInformation(
                    "Cash payment verified for order {OrderNumber} by {VerifiedBy}",
                    order.OrderNumber, request.VerifiedBy);

                return new VerifyCashPaymentResultDto
                {
                    OrderNumber = order.OrderNumber,
                    PaymentStatus = order.Payment.Status,
                    OrderStatus = order.Status,
                    VerifiedAt = DateTime.UtcNow,
                    VerifiedBy = request.VerifiedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify cash payment for order {OrderNumber}",
                    request.OrderNumber);
                throw;
            }
        }
    }

}

