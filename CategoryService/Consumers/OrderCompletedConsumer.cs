using CategoryService.Features.Products;
using MassTransit;
using MediatR;

namespace CategoryService.Consumers
{
    public class OrderCompletedConsumer : IConsumer//<OrderCompletedEvent>
    {
    //    private readonly ISender _sender;
    //    private readonly ILogger<OrderCompletedConsumer> _logger;

    //    public OrderCompletedConsumer(ISender sender, ILogger<OrderCompletedConsumer> logger)
    //    {
    //        _sender = sender;
    //        _logger = logger;
    //    }

    //    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    //    {
    //        var message = context.Message;

    //        _logger.LogInformation(
    //            "🛒 Received OrderCompletedEvent for OrderId: {OrderId} with {ItemCount} items",
    //            message.OrderId,
    //            message.Items.Count);

    //        try
    //        {
    //            foreach (var item in message.Items)
    //            {
    //                _logger.LogInformation(
    //                    "Updating sales for ProductId: {ProductId}, Quantity: {Quantity}",
    //                    item.ProductId,
    //                    item.Quantity);

    //                var command = new IncrementProductSales.Command(
    //                    item.ProductId,
    //                    item.Quantity
    //                );

    //                var result = await _sender.Send(command);

    //                if (result.IsFailure)
    //                {
    //                    _logger.LogWarning(
    //                        "Failed to update sales for ProductId: {ProductId}. Error: {Error}",
    //                        item.ProductId,
    //                        result.Error.Message);
    //                }
    //                else
    //                {
    //                    _logger.LogInformation(
    //                        "Sales updated successfully for ProductId: {ProductId}",
    //                        item.ProductId);
    //                }
    //            }

    //            _logger.LogInformation(
    //                "OrderCompletedEvent processed successfully for OrderId: {OrderId}",
    //                message.OrderId);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex,
    //                "Error processing OrderCompletedEvent for OrderId: {OrderId}",
    //                message.OrderId);
    //            throw;

    //        }    
    //    }
       
    }   
}
