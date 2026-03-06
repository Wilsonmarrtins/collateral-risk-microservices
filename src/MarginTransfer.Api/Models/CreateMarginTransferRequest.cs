namespace MarginTransfer.Api.Models;

public record CreateMarginTransferRequest(
    Guid CustomerId,
    decimal Amount,
    string FromAccount,
    string ToAccount,
    string? Currency,
    string? Reason
);