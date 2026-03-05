using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient para Positions.Api
builder.Services.AddHttpClient("positions", client =>
{
    var baseUrl = builder.Configuration["Services:PositionsBaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Services:PositionsBaseUrl is required (Positions API base URL).");

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Collateral API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// ============================
// Collateral API
// ============================
var collateral = app.MapGroup("/v1/collateral")
    .WithTags("Collateral");

// POST /v1/collateral/calculate
collateral.MapPost("/calculate", async (CalculateCollateralRequest req, IHttpClientFactory httpFactory) =>
{
    if (req.CustomerId == Guid.Empty)
        return Results.BadRequest(new { error = "CustomerId is required." });

    var http = httpFactory.CreateClient("positions");

    // Busca posições do cliente na Positions.Api
    var positions = await http.GetFromJsonAsync<List<PositionDto>>($"/v1/positions?customerId={req.CustomerId}");
    positions ??= [];

    // Regras simples de haircut (exemplo didático)
    var items = positions.Select(p =>
    {
        var exposure = p.Quantity * p.Price;

        var haircut = p.AssetType.ToUpperInvariant() switch
        {
            "CASH" => 0.00m,
            "BOND" => 0.10m,
            "EQUITY" => 0.25m,
            _ => 0.30m
        };

        var requiredCollateral = exposure * haircut;

        return new CollateralItem(
            Symbol: p.Symbol,
            AssetType: p.AssetType,
            Exposure: exposure,
            Haircut: haircut,
            RequiredCollateral: requiredCollateral,
            Currency: p.Currency
        );
    }).ToList();

    var totalExposure = items.Sum(i => i.Exposure);
    var totalRequired = items.Sum(i => i.RequiredCollateral);

    var response = new CalculateCollateralResponse(
        CustomerId: req.CustomerId,
        TotalExposure: totalExposure,
        TotalRequiredCollateral: totalRequired,
        Items: items
    );

    return Results.Ok(response);
})
.WithName("CalculateCollateral");

app.Run();

// ============================
// Contracts (requests/responses)
// ============================
record CalculateCollateralRequest(Guid CustomerId);

record CalculateCollateralResponse(
    Guid CustomerId,
    decimal TotalExposure,
    decimal TotalRequiredCollateral,
    List<CollateralItem> Items
);

record CollateralItem(
    string Symbol,
    string AssetType,
    decimal Exposure,
    decimal Haircut,
    decimal RequiredCollateral,
    string Currency
);

// DTO vindo da Positions.Api
record PositionDto(
    Guid CustomerId,
    string Symbol,
    string AssetType,
    decimal Quantity,
    decimal Price,
    string Currency,
    DateTimeOffset UpdatedAt
);