using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Features.Shared;
using UserProfileService.Infrastructure;

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
                Guid userId;
                try
                {
                    userId = httpContext.User.GetUserId();
                }
                catch
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
