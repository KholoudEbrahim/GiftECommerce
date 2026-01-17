using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Features.Commands.PlaceOrder;
using OrderService.Features.Commands.RateOrderItem;
using OrderService.Features.Commands.ReOrder;
using OrderService.Features.Commands.VerifyCashPayment;
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
        public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var orderEndpoints = app.MapGroup("/api/orders")
                .WithTags("Orders")
                .RequireAuthorization();

            orderEndpoints.MapPost("/", async (
                     [FromServices] IMediator mediator,
                     [FromServices] IUserContext userContext,
                     [FromServices] IValidator<PlaceOrderCommand> validator,
                     [FromBody] PlaceOrderRequest request,
                      CancellationToken cancellationToken) =>
            {
                var command = new PlaceOrderCommand(
                    UserId: userContext.UserId,
                    DeliveryAddressId: request.DeliveryAddressId,
                    PaymentMethod: request.PaymentMethod,
                    Notes: request.Notes
                );

                var validationResult = await validator.ValidateAsync(
                    command,
                    cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(
                        validationResult.ToDictionary());
                }

                var result = await mediator.Send(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/orders/{result.OrderNumber}",
                    ApiResponse<PlaceOrderResultDto>.SuccessResponse(
                        result,
                        "Order placed successfully"
                    ));
            })
                  .WithName("PlaceOrder")
                   .WithSummary("Create a new order from active cart")
                   .WithDescription("Creates a new order using the user's active cart")
                    .Produces<ApiResponse<PlaceOrderResultDto>>(StatusCodes.Status201Created)
                        .ProducesValidationProblem()
                       .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                      .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);


            orderEndpoints.MapGet("/", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromServices] IValidator<GetOrdersQuery> validator,
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

                var validationResult = await validator.ValidateAsync(query, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(
                    ApiResponse<GetOrdersResultDto>.SuccessResponse(result));
            })
            .WithName("GetUserOrders")
            .WithSummary("Get user's orders with pagination")
            .WithDescription("Retrieves all orders for the authenticated user")
            .Produces<ApiResponse<GetOrdersResultDto>>()
            .ProducesValidationProblem()
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

   
            orderEndpoints.MapGet("/{orderNumber}", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromServices] IValidator<GetOrderByIdQuery> validator,
                CancellationToken cancellationToken,
                string orderNumber) =>
            {
                var query = new GetOrderByIdQuery(
                    UserId: userContext.UserId,
                    OrderNumber: orderNumber
                );

                var validationResult = await validator.ValidateAsync(query, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(
                    ApiResponse<OrderDetailsDto>.SuccessResponse(result));
            })
            .WithName("GetOrderById")
            .WithSummary("Get order details by order number")
            .WithDescription("Retrieves detailed information about a specific order")
            .Produces<ApiResponse<OrderDetailsDto>>()
            .ProducesValidationProblem()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);


            orderEndpoints.MapGet("/{orderNumber}/track", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromServices] IValidator<TrackOrderQuery> validator,
                CancellationToken cancellationToken,
                string orderNumber) =>
            {
                var query = new TrackOrderQuery(
                    UserId: userContext.UserId,
                    OrderNumber: orderNumber
                );

  
                var validationResult = await validator.ValidateAsync(query, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(
                    ApiResponse<TrackingResultDto>.SuccessResponse(result));
            })
            .WithName("TrackOrder")
            .WithSummary("Track order status and delivery")
            .WithDescription("Get real-time tracking information for an order")
            .Produces<ApiResponse<TrackingResultDto>>()
            .ProducesValidationProblem()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

   
            orderEndpoints.MapPost("/{orderNumber}/reorder", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromServices] IValidator<ReOrderCommand> validator,
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


                var validationResult = await validator.ValidateAsync(command, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await mediator.Send(command, cancellationToken);

                return Results.Created(
                    $"/api/orders/{result.NewOrderNumber}",
                    ApiResponse<ReOrderResultDto>.SuccessResponse(
                        result,
                        "Order created successfully from previous order"
                    ));
            })
            .WithName("ReOrder")
            .WithSummary("Create new order from previous order")
            .WithDescription("Reorder items from a previously delivered order")
            .Produces<ApiResponse<ReOrderResultDto>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

 
            orderEndpoints.MapPost("/items/{orderItemId}/rate", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromServices] IValidator<RateOrderItemCommand> validator,
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


                var validationResult = await validator.ValidateAsync(command, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(
                    ApiResponse<RateOrderItemResultDto>.SuccessResponse(
                        result,
                        "Product rated successfully"
                    ));
            })
            .WithName("RateOrderItem")
            .WithSummary("Rate a product from delivered order")
            .WithDescription("Submit rating and review for a product from a delivered order")
            .Produces<ApiResponse<RateOrderItemResultDto>>()
            .ProducesValidationProblem()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized);

            orderEndpoints.MapPost("/{orderNumber}/verify-cash-payment", async (
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                [FromServices] IValidator<VerifyCashPaymentCommand> validator,
                CancellationToken cancellationToken,
                string orderNumber,
                [FromBody] VerifyCashPaymentRequest? request = null) =>
            {
                var command = new VerifyCashPaymentCommand(
                    OrderNumber: orderNumber,
                    VerifiedBy: userContext.UserId,
                    TransactionId: request?.TransactionId
                );

                var validationResult = await validator.ValidateAsync(command, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(
                    ApiResponse<VerifyCashPaymentResultDto>.SuccessResponse(
                        result,
                        "Cash payment verified successfully"
                    ));
            })
            .WithName("VerifyCashPayment")
            .WithSummary("Verify cash on delivery payment (Admin/Delivery Hero only)")
            .WithDescription("Confirms that cash payment was received from customer")
            .RequireAuthorization("AdminOrDeliveryHero") 
            .Produces<ApiResponse<VerifyCashPaymentResultDto>>()
            .ProducesValidationProblem()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

            return app;
        }
    }
}