namespace MarginTransfer.Api.Models;

public record AccountLedger(
    Guid CustomerId,
    decimal CashBalance,
    decimal MarginBalance,
    DateTimeOffset UpdatedAt
)
{
    public object LockObj { get; } = new();
}