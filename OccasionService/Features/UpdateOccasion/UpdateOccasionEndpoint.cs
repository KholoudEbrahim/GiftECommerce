using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OccasionService.Features.UpdateOccasion
{
    public static class UpdateOccasionEndpoint
    {
        public static IEndpointRouteBuilder MapUpdateOccasion(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/occasions/{id:guid}", async (
                Guid id,
                [FromBody] UpdateOccasionRequest request,
                IMediator mediator) =>
            {
                var command = new UpdateOccasionCommand
                {
                    Id = id,
                    Name = request.Name,
                    IsActive = request.IsActive,
                    ImageUrl = request.ImageUrl
                };

                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }

                return result.Error.Code switch
                {
                    "NOT_FOUND" => Results.NotFound(new { error = result.Error.Message }),
                    "DUPLICATE_NAME" => Results.Conflict(new { error = result.Error.Message }),
                    _ => Results.BadRequest(new { error = result.Error.Message })
                };
            })
            .WithName("UpdateOccasion")
            .WithTags("Occasions")
            .Produces(204)
            .Produces(404)
            .Produces(409);

            return app;
        }
    }
}
