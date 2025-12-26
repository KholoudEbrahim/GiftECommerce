using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Features.Commands.PlaceOrder;
using OrderService.Features.Commands.RateOrderItem;
using OrderService.Features.Commands.ReOrder;
using OrderService.Features.Endpoints.DTOs;
using OrderService.Features.Queries.GetOrderById;
using OrderService.Features.Queries.GetOrders;
using OrderService.Features.Queries.TrackOrder;
using OrderService.Features.Shared;
using OrderService.Features.Tracking.DTOs;
namespace OrderService.Features.Endpoints
{
    public static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var orderEndpoints = app.MapGroup("/api/orders")
                .WithTags("Orders")
                .RequireAuthorization();

        
            orderEndpoints.MapPost("/", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromBody] PlaceOrderRequest request,
                CancellationToken cancellationToken) =>
            {
                var command = new PlaceOrderCommand(
                    UserId: userContext.UserId,
                    DeliveryAddressId: request.DeliveryAddressId,
                    PaymentMethod: request.PaymentMethod,
                    CartId: request.CartId,
                    Notes: request.Notes
                );

                var result = await mediator.Send(command, cancellationToken);

                return Results.Created($"/api/orders/{result.OrderNumber}",
                    ApiResponse<PlaceOrderResultDto>.SuccessResponse(result));
            })
            .WithName("PlaceOrder")
            .Produces<ApiResponse<PlaceOrderResultDto>>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

            orderEndpoints.MapGet("/", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken,
                [FromQuery] bool? activeOnly = null,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20) =>
            {
                var query = new GetOrdersQuery(
                    UserId: userContext.UserId,
                    ActiveOnly: activeOnly,
                    Page: page,
                    PageSize: pageSize
                );

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(ApiResponse<GetOrdersResultDto>.SuccessResponse(result));
            })
            .WithName("GetUserOrders")
            .Produces<ApiResponse<GetOrdersResultDto>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

    
            orderEndpoints.MapGet("/{orderNumber}", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken,
                string orderNumber) =>
            {
                var query = new GetOrderByIdQuery(
                    UserId: userContext.UserId,
                    OrderNumber: orderNumber
                );

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(ApiResponse<OrderDetailsDto>.SuccessResponse(result));
            })
            .WithName("GetOrderById")
            .Produces<ApiResponse<OrderDetailsDto>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

     
            orderEndpoints.MapGet("/{orderNumber}/track", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken,
                string orderNumber) =>
            {
                var query = new TrackOrderQuery(
                    UserId: userContext.UserId,
                    OrderNumber: orderNumber
                );

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(ApiResponse<TrackingResultDto>.SuccessResponse(result));
            })
            .WithName("TrackOrder")
            .Produces<ApiResponse<TrackingResultDto>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

     
            orderEndpoints.MapPost("/{orderNumber}/reorder", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromBody] ReOrderRequest? request,
                CancellationToken cancellationToken,
                string orderNumber) =>
            {
         
                var command = new ReOrderCommand(
                    UserId: userContext.UserId,
                    OrderNumber: orderNumber,
                    NewAddressId: request?.NewAddressId,
                    ModifiedItems: request?.ModifiedItems
                );

                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(ApiResponse<ReOrderResultDto>.SuccessResponse(result));
            })
            .WithName("ReOrder")
            .Produces<ApiResponse<ReOrderResultDto>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

          
            orderEndpoints.MapPost("/items/{orderItemId}/rate", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromBody] RateOrderItemRequest request,
                CancellationToken cancellationToken,
                int orderItemId) =>
            {
                var command = new RateOrderItemCommand(
                    UserId: userContext.UserId,
                    OrderItemId: orderItemId,
                    Rating: request.Rating,
                    Comment: request.Comment
                );

                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(ApiResponse<RateOrderItemResultDto>.SuccessResponse(result));
            })
            .WithName("RateOrderItem")
            .Produces<ApiResponse<RateOrderItemResultDto>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);
        }
    }
}
