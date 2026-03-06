namespace MarginTransfer.Api.Models;

public record SimpleTransferResult(
    MarginTransferEntry Transfer,
    AccountLedger LedgerAfter
);