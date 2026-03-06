using Customers.Api.Models;
using Customers.Api.Services;

namespace Customers.Api.Endpoints;

public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder app)
    {
        var customers = app.MapGroup("/v1/customers")
            .WithTags("Customers");

        customers.MapPost("/", (CreateCustomerRequest req, CustomerService service) =>
        {
            var result = service.Create(req);

            return result.IsSuccess
                ? Results.Created($"/v1/customers/{result.Value!.Id}", new { id = result.Value.Id })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateCustomer");

        customers.MapGet("/{id:guid}", (Guid id, CustomerService service) =>
        {
            var customer = service.GetById(id);

            return customer is not null
                ? Results.Ok(customer)
                : Results.NotFound();
        })
        .WithName("GetCustomerById");

        customers.MapGet("/", (CustomerService service) =>
        {
            var list = service.List();
            return Results.Ok(list);
        })
        .WithName("ListCustomers");

        return app;
    }
}