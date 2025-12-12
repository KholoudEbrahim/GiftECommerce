using MediatR;
using Microsoft.AspNetCore.Mvc;
using OccasionService.Features.CreateOccasion;

namespace OccasionService.Features.CreateOccasion
{
    public static class CreateOccasionEndpoint
    {
        public static IEndpointRouteBuilder MapCreateOccasion(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/occasions", async (
                [FromBody] CreateOccasionRequest request,
                IMediator mediator) =>
            {
                var command = new CreateOccasionCommand
                {
                    Name = request.Name,
                    IsActive = request.IsActive,
                    ImageUrl = request.ImageUrl
                };
                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    return Results.Created(
                        $"/api/occasions/{result.Value}",
                        new { id = result.Value });
                }

                return result.Error.Code switch
                {
                    "DUPLICATE_NAME" => Results.Conflict(new { error = result.Error.Message }),
                    _ => Results.BadRequest(new { error = result.Error.Message })
                };
            })
            .WithName("CreateOccasion")
            .WithTags("Occasions")
            .Produces(201)
            .Produces(400)
            .Produces(409);

            return app;
        }
    }
}
