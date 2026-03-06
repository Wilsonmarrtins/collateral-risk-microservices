using Positions.Api.Models;
using Positions.Api.Services;

namespace Positions.Api.Endpoints;

public static class PositionsEndpoints
{
    public static IEndpointRouteBuilder MapPositionsEndpoints(this IEndpointRouteBuilder app)
    {
        var positions = app.MapGroup("/v1/positions")
            .WithTags("Positions");

        positions.MapPost("/", (UpsertPositionRequest req, PositionService service) =>
        {
            var result = service.Upsert(req);

            return result.IsSuccess
                ? Results.Ok(new { message = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpsertPosition");

        positions.MapGet("/", (Guid customerId, PositionService service) =>
        {
            var result = service.ListByCustomer(customerId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ListPositionsByCustomer");

        return app;
    }
}