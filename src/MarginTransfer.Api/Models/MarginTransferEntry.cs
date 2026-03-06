namespace MarginTransfer.Api.Models;

public record MarginTransferEntry(
    Guid Id,
    Guid CustomerId,
    string Type,
    string FromAccount,
    string ToAccount,
    decimal Amount,
    string Currency,
    string Reason,
    DateTimeOffset CreatedAt
);