using System.Collections.Concurrent;
using CollateralRisk.BuildingBlocks.Results;
using Customers.Api.Models;

namespace Customers.Api.Services;

public sealed class CustomerService
{
    private readonly ConcurrentDictionary<Guid, Customer> _customers = new();

    public ServiceResult<Customer> Create(CreateCustomerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return ServiceResult<Customer>.Fail("Name is required.");

        if (string.IsNullOrWhiteSpace(req.Document))
            return ServiceResult<Customer>.Fail("Document is required.");

        var customer = new Customer(
            Id: Guid.NewGuid(),
            Name: req.Name.Trim(),
            Document: req.Document.Trim(),
            CreatedAt: DateTimeOffset.UtcNow
        );

        _customers[customer.Id] = customer;

        return ServiceResult<Customer>.Success(customer);
    }

    public Customer? GetById(Guid id)
    {
        return _customers.TryGetValue(id, out var customer)
            ? customer
            : null;
    }

    public List<Customer> List()
    {
        return _customers.Values
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }
}