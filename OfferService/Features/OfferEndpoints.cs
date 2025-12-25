using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OfferService.Features;

public class OfferEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offers").WithTags("Offers");

        // US-G01: Create
        group.MapPost("/", async ([FromBody] CreateOffer.Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/offers/{result.Value}", result.Value)
                : Results.BadRequest(result.Error);
        });

        // US-G05: Check for Discount
        // Usage: GET /api/offers/check?productId=1&categoryId=2&occasionId=3
        group.MapGet("/check", async (
            [FromQuery] int? productId,
            [FromQuery] int? categoryId,
            [FromQuery] int? occasionId,
            ISender sender) =>
        {
            var query = new GetApplicableOffer.Query(productId, categoryId, occasionId);
            var result = await sender.Send(query);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Error);
        });


        // 3. Update Offer
        group.MapPut("/{id}", async (int id, [FromBody] UpdateOffer.UpdateOffer.Command command, ISender sender) =>
        {
            if (id != command.Id) return Results.BadRequest("Route ID mismatch");

            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // 4. Delete Offer
        group.MapDelete("/{id}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteOffer.DeleteOffer.Command(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

     
    }
}