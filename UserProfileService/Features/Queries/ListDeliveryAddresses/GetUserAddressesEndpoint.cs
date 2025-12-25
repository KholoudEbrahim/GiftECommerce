using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Queries.ListDeliveryAddresses
{
    public static class GetUserAddressesEndpoint
    {
        public static void MapGetUserAddressesEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("api/profile/addresses", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken, 
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                // Get user ID from claims
                var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var query = new GetUserAddressesQuery(userId, pageNumber, pageSize);

                var result = await mediator.Send(query, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .RequireAuthorization()
            .WithName("GetUserAddresses")
            .Produces<ApiResponse<PaginatedResponse<AddressDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .WithTags("Addresses");
        }
    }
}
