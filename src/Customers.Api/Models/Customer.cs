namespace Customers.Api.Models;

public record Customer(
    Guid Id,
    string Name,
    string Document,
    DateTimeOffset CreatedAt
);