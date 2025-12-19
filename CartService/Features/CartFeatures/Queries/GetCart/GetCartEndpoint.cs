using CartService.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Features.CartFeatures.Queries.GetCart
{
    public static class GetCartEndpoint
    {
        public static void MapGetCartEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cart", async (
                [FromQuery] bool includeAddressDetails,
                [FromServices] IMediator mediator,
                [FromServices] IUserContext userContext,
                CancellationToken cancellationToken) =>
            {
                var query = new GetCart.GetCartQuery
                (
                    userContext.UserId,
                    userContext.AnonymousId,
                  includeAddressDetails
                );

                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(ApiResponse<GetCart.CartDetailsDto>.SuccessResponse(result));
            })
            .WithName("GetCart")
            .WithTags("Cart")
            .Produces<ApiResponse<GetCart.CartDetailsDto>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization("OptionalAuth");
        }
    }
}
