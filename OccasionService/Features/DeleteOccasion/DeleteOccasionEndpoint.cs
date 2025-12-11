using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OccasionService.Features.DeleteOccasion
{
    public static class DeleteOccasionEndpoint
    {

        public static IEndpointRouteBuilder MapDeleteOccasion(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/occasions/{id:guid}", async (
                Guid id,
                IMediator mediator) =>
            {
                var command = new DeleteOccasionCommand { Id = id };
                var result = await mediator.Send(command);

                if (result.IsSuccess)
                { 
                    return Results.NoContent();
                }

                return result.Error.Code switch
                {
                    "NOT_FOUND" => Results.NotFound(new { error = result.Error.Message }),
                    "HAS_DEPENDENCIES" => Results.BadRequest(new { error = result.Error.Message }),
                    _ => Results.BadRequest(new { error = result.Error.Message })
                };
            })
            .WithName("DeleteOccasion")
            .WithTags("Occasions")
            .Produces(204)
            .Produces(404)
            .Produces(400);

            return app;
        }
    }
}
