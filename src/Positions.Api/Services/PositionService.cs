using System.Collections.Concurrent;
using CollateralRisk.BuildingBlocks.Results;
using Positions.Api.Models;

namespace Positions.Api.Services;

public sealed class PositionService
{
    private readonly ConcurrentDictionary<Guid, List<Position>> _positions = new();

    public ServiceResult<string> Upsert(UpsertPositionRequest req)
    {
        if (req.CustomerId == Guid.Empty)
            return ServiceResult<string>.Fail("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(req.Symbol))
            return ServiceResult<string>.Fail("Symbol is required.");

        if (string.IsNullOrWhiteSpace(req.AssetType))
            return ServiceResult<string>.Fail("AssetType is required. Use CASH, BOND, EQUITY.");

        if (req.Quantity <= 0)
            return ServiceResult<string>.Fail("Quantity must be > 0.");

        if (req.Price <= 0)
            return ServiceResult<string>.Fail("Price must be > 0.");

        var list = _positions.GetOrAdd(req.CustomerId, _ => new List<Position>());

        lock (list)
        {
            list.RemoveAll(p => p.Symbol.Equals(req.Symbol, StringComparison.OrdinalIgnoreCase));

            list.Add(new Position(
                CustomerId: req.CustomerId,
                Symbol: req.Symbol.Trim().ToUpperInvariant(),
                AssetType: req.AssetType.Trim().ToUpperInvariant(),
                Quantity: req.Quantity,
                Price: req.Price,
                Currency: string.IsNullOrWhiteSpace(req.Currency)
                    ? "USD"
                    : req.Currency.Trim().ToUpperInvariant(),
                UpdatedAt: DateTimeOffset.UtcNow
            ));
        }

        return ServiceResult<string>.Success("Position upserted.");
    }

    public ServiceResult<List<Position>> ListByCustomer(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return ServiceResult<List<Position>>.Fail("customerId query param is required.");

        var list = _positions.TryGetValue(customerId, out var positions)
            ? positions.OrderByDescending(p => p.UpdatedAt).ToList()
            : new List<Position>();

        return ServiceResult<List<Position>>.Success(list);
    }
}