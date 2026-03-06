namespace MarginTransfer.Api.Models;

public record InitAccountRequest(
    Guid CustomerId,
    decimal CashBalance,
    decimal MarginBalance
);