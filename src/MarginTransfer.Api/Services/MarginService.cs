using System.Collections.Concurrent;
using CollateralRisk.BuildingBlocks.Results;
using MarginTransfer.Api.Models;

namespace MarginTransfer.Api.Services;

public sealed class MarginService
{
    private readonly ConcurrentDictionary<Guid, AccountLedger> _accounts = new();
    private readonly ConcurrentDictionary<Guid, List<MarginTransferEntry>> _transfers = new();

    public ServiceResult<AccountLedger> InitAccount(InitAccountRequest req)
    {
        if (req.CustomerId == Guid.Empty)
            return ServiceResult<AccountLedger>.Fail("CustomerId is required.");

        if (req.CashBalance < 0 || req.MarginBalance < 0)
            return ServiceResult<AccountLedger>.Fail("Balances must be >= 0.");

        var ledger = new AccountLedger(
            CustomerId: req.CustomerId,
            CashBalance: Round2(req.CashBalance),
            MarginBalance: Round2(req.MarginBalance),
            UpdatedAt: DateTimeOffset.UtcNow
        );

        _accounts[req.CustomerId] = ledger;

        return ServiceResult<AccountLedger>.Success(ledger);
    }

    public ServiceResult<AccountLedger> GetAccount(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return ServiceResult<AccountLedger>.Fail("customerId query param is required.");

        var ledger = _accounts.TryGetValue(customerId, out var acc)
            ? acc
            : new AccountLedger(customerId, 0m, 0m, DateTimeOffset.UtcNow);

        return ServiceResult<AccountLedger>.Success(ledger);
    }

    public ServiceResult<SimpleTransferResult> SimpleTransfer(CreateMarginTransferRequest req)
    {
        if (req.CustomerId == Guid.Empty)
            return ServiceResult<SimpleTransferResult>.Fail("CustomerId is required.");

        if (req.Amount <= 0)
            return ServiceResult<SimpleTransferResult>.Fail("Amount must be > 0.");

        var from = Normalize(req.FromAccount);
        var to = Normalize(req.ToAccount);

        var allowed = (from == "CASH" && to == "MARGIN") ||
                      (from == "MARGIN" && to == "CASH");

        if (!allowed)
        {
            return ServiceResult<SimpleTransferResult>.Fail(
                "Invalid transfer. Use FromAccount=CASH ToAccount=MARGIN or FromAccount=MARGIN ToAccount=CASH.");
        }

        var ledger = GetOrCreateLedger(req.CustomerId);

        lock (ledger.LockObj)
        {
            var amount = Round2(req.Amount);

            decimal cash = ledger.CashBalance;
            decimal margin = ledger.MarginBalance;

            if (from == "CASH" && cash < amount)
            {
                return ServiceResult<SimpleTransferResult>.Fail(
                    "Insufficient CASH balance.",
                    new { cashBalance = cash, marginBalance = margin });
            }

            if (from == "MARGIN" && margin < amount)
            {
                return ServiceResult<SimpleTransferResult>.Fail(
                    "Insufficient MARGIN balance.",
                    new { cashBalance = cash, marginBalance = margin });
            }

            if (from == "CASH")
            {
                cash -= amount;
                margin += amount;
            }
            else
            {
                margin -= amount;
                cash += amount;
            }

            var updated = ledger with
            {
                CashBalance = Round2(cash),
                MarginBalance = Round2(margin),
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _accounts[req.CustomerId] = updated;

            var transfer = new MarginTransferEntry(
                Id: Guid.NewGuid(),
                CustomerId: req.CustomerId,
                Type: "SIMPLE_TRANSFER",
                FromAccount: from,
                ToAccount: to,
                Amount: amount,
                Currency: string.IsNullOrWhiteSpace(req.Currency)
                    ? "BRL"
                    : req.Currency.Trim().ToUpperInvariant(),
                Reason: string.IsNullOrWhiteSpace(req.Reason)
                    ? "MARGIN_ALLOCATION"
                    : req.Reason.Trim(),
                CreatedAt: DateTimeOffset.UtcNow
            );

            var list = _transfers.GetOrAdd(req.CustomerId, _ => new List<MarginTransferEntry>());

            lock (list)
            {
                list.Add(transfer);
            }

            return ServiceResult<SimpleTransferResult>.Success(
                new SimpleTransferResult(transfer, updated));
        }
    }

    public ServiceResult<List<MarginTransferEntry>> ListTransfers(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return ServiceResult<List<MarginTransferEntry>>.Fail("customerId query param is required.");

        var list = _transfers.TryGetValue(customerId, out var txs)
            ? txs.OrderByDescending(x => x.CreatedAt).ToList()
            : new List<MarginTransferEntry>();

        return ServiceResult<List<MarginTransferEntry>>.Success(list);
    }

    private AccountLedger GetOrCreateLedger(Guid customerId)
    {
        return _accounts.GetOrAdd(customerId, _ => new AccountLedger(
            CustomerId: customerId,
            CashBalance: 0m,
            MarginBalance: 0m,
            UpdatedAt: DateTimeOffset.UtcNow
        ));
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static decimal Round2(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}