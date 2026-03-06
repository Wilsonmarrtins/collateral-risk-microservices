using MarginTransfer.Api.Models;
using MarginTransfer.Api.Services;

namespace MarginTransfer.Api.Endpoints;

public static class MarginEndpoints
{
    public static IEndpointRouteBuilder MapMarginEndpoints(this IEndpointRouteBuilder app)
    {
        var margin = app.MapGroup("/v1/margin")
            .WithTags("Margin");

        margin.MapPost("/accounts/init", (InitAccountRequest req, MarginService service) =>
        {
            var result = service.InitAccount(req);

            return result.IsSuccess
                ? Results.Ok(new { message = "Account ledger initialized.", ledger = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("InitAccount");

        margin.MapGet("/accounts", (Guid customerId, MarginService service) =>
        {
            var result = service.GetAccount(customerId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetAccount");

        margin.MapPost("/transfers/simple", (CreateMarginTransferRequest req, MarginService service) =>
        {
            var result = service.SimpleTransfer(req);

            return result.IsSuccess
                ? Results.Ok(new
                {
                    message = "Transfer executed.",
                    transfer = result.Value!.Transfer,
                    ledgerAfter = result.Value.LedgerAfter
                })
                : Results.BadRequest(new { error = result.Error, details = result.Metadata });
        })
        .WithName("SimpleTransfer");

        margin.MapGet("/transfers", (Guid customerId, MarginService service) =>
        {
            var result = service.ListTransfers(customerId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ListTransfers");

        return app;
    }
}