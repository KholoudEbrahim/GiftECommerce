using MediatR;

namespace OccasionService.Features.ToggleOccasionStatus
{
    public static class ToggleOccasionStatusEndpoint
    {
        public static IEndpointRouteBuilder MapToggleOccasionStatus(this IEndpointRouteBuilder app)
        {

            // Activate
            app.MapPatch("/api/occasions/{id:guid}/activate", async (
                Guid id,
                IMediator mediator) =>
            {
                var command = new ToggleOccasionStatusCommand
                {
                    Id = id,
                    IsActive = true
                };

                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }

                return result.Error.Code == "NOT_FOUND"
                    ? Results.NotFound(new { error = result.Error.Message })
                    : Results.BadRequest(new { error = result.Error.Message });
            })
            .WithName("ActivateOccasion")
            .WithTags("Occasions")
            .Produces(204)
            .Produces(404);

            // Deactivate
            app.MapPatch("/api/occasions/{id:guid}/deactivate", async (
                Guid id,
                IMediator mediator) =>
            {
                var command = new ToggleOccasionStatusCommand
                {
                    Id = id,
                    IsActive = false
                };

                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    return Results.NoContent();
                }

                return result.Error.Code == "NOT_FOUND"
                    ? Results.NotFound(new { error = result.Error.Message })
                    : Results.BadRequest(new { error = result.Error.Message });
            })
            .WithName("DeactivateOccasion")
            .WithTags("Occasions")
            .Produces(204)
            .Produces(404);

            return app;
        }
    }
}
