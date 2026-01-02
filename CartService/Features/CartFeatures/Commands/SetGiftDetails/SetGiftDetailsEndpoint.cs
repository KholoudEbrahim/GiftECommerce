using CartService.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Features.CartFeatures.Commands.SetGiftDetails
{
    public static class SetGiftDetailsEndpoint
    {
        public static void MapSetGiftDetailsEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cart/gift-details", async (
                [FromBody] SetGiftDetailsRequest request,
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {
                var command = new SetGiftDetailsCommand(
                    userContext.UserId,
                    userContext.AnonymousId,
                    request.RecipientName,
                    request.RecipientPhone,
                    request.GiftMessage,
                    request.DeliveryDate,
                    request.GiftWrap);

                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(ApiResponse<SetGiftDetailsResult>.SuccessResponse(result));
            })
            .WithName("SetGiftDetails")
            .WithTags("Cart")
            .Produces<ApiResponse<SetGiftDetailsResult>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization("OptionalAuth");
        }

    }
}
