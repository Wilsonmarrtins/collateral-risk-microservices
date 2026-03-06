namespace Collateral.Api.Models;

public record CollateralItem(
    string Symbol,
    string AssetType,
    decimal Exposure,
    decimal Haircut,
    decimal RequiredCollateral,
    string Currency
);