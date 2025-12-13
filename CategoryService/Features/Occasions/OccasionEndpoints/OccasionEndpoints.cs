using Carter;
using CategoryService.Contracts.Occasion;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Occasions.OccasionEndpoints
{
    public class OccasionEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/occasions").WithTags("Occasions");

            // 1. Create Occasion
            group.MapPost("/", async ([FromBody] CreateOccasionRequest request, ISender sender) =>
            {
                var command = new CreateOccasion.CreateOccasionCommand(request.Name, request.ImageUrl, request.IsActive);
                Result<int> result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Created($"/api/occasions/{result.Value}", result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("CreateOccasion")
            .WithSummary("Creates a new occasion");

            // 2. Get All Occasions
            group.MapGet("/", async (ISender sender) =>
            {
                var query = new GetOccasions.Query();
                Result<IEnumerable<GetOccasionsResponse>> result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetOccasions")
            .WithSummary("Retrieves a list of all occasions");

            // 3. Get Occasion Details (with Products)
            group.MapGet("/{id}", async (int id, ISender sender) =>
            {
                var query = new GetOccasionDetails.Query(id);
                Result<GetOccasionDetailsResponse> result = await sender.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            })
            .WithName("GetOccasionDetails")
            .WithSummary("Retrieves detailed information about a specific occasion");

            // 4. Update Occasion
            group.MapPut("/{id}", async (int id, [FromBody] UpdateOccasionRequest request, ISender sender) =>
            {
                if (id != request.Id)
                    return Results.BadRequest("Route ID does not match Body ID");

                var command = new UpdateOccasion.UpdateOccasionCommand(request.Id, request.Name, request.ImageUrl, request.IsActive);
                Result<bool> result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            })
            .WithName("UpdateOccasion")
            .WithSummary("Updates an existing occasion");

            // 5. Delete Occasion
            group.MapDelete("/{id}", async (int id, ISender sender) =>
            {
                var command = new DeleteOccasion.DeleteOccasionCommand(id);
                Result<bool> result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.BadRequest(result.Error);
            })
            .WithName("DeleteOccasion")
            .WithSummary("Deletes an occasion if it has no dependencies");

            // 6. Activate Occasion
            group.MapPatch("/{id}/activate", async (int id, ISender sender) =>
            {
                var command = new ToggleOccasionStatus.ToggleOccasionStatusCommand(id, true);
                Result<int> result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Occasion activated successfully" })
                    : Results.BadRequest(result.Error);
            })
            .WithName("ActivateOccasion")
            .WithSummary("Activates an occasion");

            // 7. Deactivate Occasion
            group.MapPatch("/{id}/deactivate", async (int id, ISender sender) =>
            {
                var command = new ToggleOccasionStatus.ToggleOccasionStatusCommand(id, false);
                Result<int> result = await sender.Send(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Occasion deactivated successfully" })
                    : Results.BadRequest(result.Error);
            })
            .WithName("DeactivateOccasion")
            .WithSummary("Deactivates an occasion");
        }
    }
}
