namespace Positions.Api.Models;

public record UpsertPositionRequest(
    Guid CustomerId,
    string Symbol,
    string AssetType,
    decimal Quantity,
    decimal Price,
    string? Currency
);