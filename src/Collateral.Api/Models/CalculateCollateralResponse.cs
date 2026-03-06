namespace Collateral.Api.Models;

public record CalculateCollateralResponse(
    Guid CustomerId,
    decimal TotalExposure,
    decimal TotalRequiredCollateral,
    List<CollateralItem> Items
);