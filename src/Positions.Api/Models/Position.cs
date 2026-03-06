namespace Positions.Api.Models;

public record Position(
    Guid CustomerId,
    string Symbol,
    string AssetType,
    decimal Quantity,
    decimal Price,
    string Currency,
    DateTimeOffset UpdatedAt
);