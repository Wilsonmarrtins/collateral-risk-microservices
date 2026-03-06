using Positions.Api.Endpoints;
using Positions.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PositionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Positions API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapPositionsEndpoints();

app.Run();


//using System.Collections.Concurrent;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//var positionsDb = new ConcurrentDictionary<Guid, List<Position>>();

//var positions = app.MapGroup("/v1/positions")
//    .WithTags("Positions");

//positions.MapPost("/", (UpsertPositionRequest req) =>
//{
//    if (req.CustomerId == Guid.Empty)
//        return Results.BadRequest(new { error = "CustomerId is required." });

//    if (string.IsNullOrWhiteSpace(req.Symbol))
//        return Results.BadRequest(new { error = "Symbol is required." });

//    if (string.IsNullOrWhiteSpace(req.AssetType))
//        return Results.BadRequest(new { error = "AssetType is required. Use CASH, BOND, EQUITY." });

//    if (req.Quantity <= 0)
//        return Results.BadRequest(new { error = "Quantity must be > 0." });

//    if (req.Price <= 0)
//        return Results.BadRequest(new { error = "Price must be > 0." });

//    var list = positionsDb.GetOrAdd(req.CustomerId, _ => new List<Position>());

//    list.RemoveAll(p => p.Symbol.Equals(req.Symbol, StringComparison.OrdinalIgnoreCase));

//    list.Add(new Position(
//        CustomerId: req.CustomerId,
//        Symbol: req.Symbol.Trim().ToUpperInvariant(),
//        AssetType: req.AssetType.Trim().ToUpperInvariant(),
//        Quantity: req.Quantity,
//        Price: req.Price,
//        Currency: (req.Currency ?? "USD").Trim().ToUpperInvariant(),
//        UpdatedAt: DateTimeOffset.UtcNow
//    ));

//    return Results.Ok(new { message = "Position upserted." });
//})
//.WithName("UpsertPosition");

//positions.MapGet("/", (Guid customerId) =>
//{
//    if (customerId == Guid.Empty)
//        return Results.BadRequest(new { error = "customerId query param is required." });

//    var list = positionsDb.TryGetValue(customerId, out var positions)
//        ? positions.OrderByDescending(p => p.UpdatedAt).ToList()
//        : new List<Position>();

//    return Results.Ok(list);
//})
//.WithName("ListPositionsByCustomer");

//app.Run();

//record UpsertPositionRequest(
//    Guid CustomerId,
//    string Symbol,
//    string AssetType, // CASH | BOND | EQUITY
//    decimal Quantity,
//    decimal Price,
//    string? Currency
//);

//record Position(
//    Guid CustomerId,
//    string Symbol,
//    string AssetType,
//    decimal Quantity,
//    decimal Price,
//    string Currency,
//    DateTimeOffset UpdatedAt
//);