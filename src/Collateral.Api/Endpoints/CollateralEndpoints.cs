using Collateral.Api.Models;
using Collateral.Api.Services;

namespace Collateral.Api.Endpoints;

public static class CollateralEndpoints
{
    public static IEndpointRouteBuilder MapCollateralEndpoints(this IEndpointRouteBuilder app)
    {
        var collateral = app.MapGroup("/v1/collateral")
            .WithTags("Collateral");

        collateral.MapPost("/calculate", async (
            CalculateCollateralRequest req,
            PositionsClient positionsClient,
            CollateralCalculator calculator) =>
        {
            if (req.CustomerId == Guid.Empty)
                return Results.BadRequest(new { error = "CustomerId is required." });

            var positionsResult = await positionsClient.GetPositionsByCustomerAsync(req.CustomerId);

            if (!positionsResult.IsSuccess)
                return Results.BadRequest(new { error = positionsResult.Error });

            var response = calculator.Calculate(req.CustomerId, positionsResult.Value!);

            return Results.Ok(response);
        })
        .WithName("CalculateCollateral");

        return app;
    }
}