namespace Collateral.Api.Models;

public record PositionDto(
    Guid CustomerId,
    string Symbol,
    string AssetType,
    decimal Quantity,
    decimal Price,
    string Currency,
    DateTimeOffset UpdatedAt
);