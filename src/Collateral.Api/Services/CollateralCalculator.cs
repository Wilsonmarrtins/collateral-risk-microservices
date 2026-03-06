using Collateral.Api.Models;

namespace Collateral.Api.Services;

public sealed class CollateralCalculator
{
    public CalculateCollateralResponse Calculate(Guid customerId, List<PositionDto> positions)
    {
        var items = positions.Select(p =>
        {
            var exposure = p.Quantity * p.Price;

            var haircut = GetHaircut(p.AssetType);

            var requiredCollateral = exposure * haircut;

            return new CollateralItem(
                Symbol: p.Symbol,
                AssetType: p.AssetType,
                Exposure: exposure,
                Haircut: haircut,
                RequiredCollateral: requiredCollateral,
                Currency: p.Currency
            );
        }).ToList();

        var totalExposure = items.Sum(i => i.Exposure);
        var totalRequired = items.Sum(i => i.RequiredCollateral);

        return new CalculateCollateralResponse(
            CustomerId: customerId,
            TotalExposure: totalExposure,
            TotalRequiredCollateral: totalRequired,
            Items: items
        );
    }

    private static decimal GetHaircut(string assetType)
    {
        return assetType.Trim().ToUpperInvariant() switch
        {
            "CASH" => 0.00m,
            "BOND" => 0.10m,
            "EQUITY" => 0.25m,
            _ => 0.30m
        };
    }
}