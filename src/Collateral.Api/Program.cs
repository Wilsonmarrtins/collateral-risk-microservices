using Collateral.Api.Endpoints;
using Collateral.Api.Services;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<PositionsClient>(client =>
{
    var baseUrl = builder.Configuration["Services:PositionsBaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Services:PositionsBaseUrl is required.");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddSingleton<CollateralCalculator>();

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

app.MapCollateralEndpoints();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine(
                    $"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(15),
            onBreak: (outcome, breakDelay) =>
            {
                Console.WriteLine(
                    $"Circuit opened for {breakDelay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit closed. Calls to Positions API resumed.");
            });
}


//using System.Net.Http.Json;

//var builder = WebApplication.CreateBuilder(args);

//// Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// HttpClient para Positions.Api
//builder.Services.AddHttpClient("positions", client =>
//{
//    var baseUrl = builder.Configuration["Services:PositionsBaseUrl"];
//    if (string.IsNullOrWhiteSpace(baseUrl))
//        throw new InvalidOperationException("Services:PositionsBaseUrl is required (Positions API base URL).");

//    client.BaseAddress = new Uri(baseUrl);
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Collateral API v1");
//        options.RoutePrefix = "swagger";
//    });
//}

//app.UseHttpsRedirection();

//// ============================
//// Collateral API
//// ============================
//var collateral = app.MapGroup("/v1/collateral")
//    .WithTags("Collateral");

//// POST /v1/collateral/calculate
//collateral.MapPost("/calculate", async (CalculateCollateralRequest req, IHttpClientFactory httpFactory) =>
//{
//    if (req.CustomerId == Guid.Empty)
//        return Results.BadRequest(new { error = "CustomerId is required." });

//    var http = httpFactory.CreateClient("positions");

//    // Busca posições do cliente na Positions.Api
//    var positions = await http.GetFromJsonAsync<List<PositionDto>>($"/v1/positions?customerId={req.CustomerId}");
//    positions ??= [];

//    // Regras simples de haircut (exemplo didático)
//    var items = positions.Select(p =>
//    {
//        var exposure = p.Quantity * p.Price;

//        var haircut = p.AssetType.ToUpperInvariant() switch
//        {
//            "CASH" => 0.00m,
//            "BOND" => 0.10m,
//            "EQUITY" => 0.25m,
//            _ => 0.30m
//        };

//        var requiredCollateral = exposure * haircut;

//        return new CollateralItem(
//            Symbol: p.Symbol,
//            AssetType: p.AssetType,
//            Exposure: exposure,
//            Haircut: haircut,
//            RequiredCollateral: requiredCollateral,
//            Currency: p.Currency
//        );
//    }).ToList();

//    var totalExposure = items.Sum(i => i.Exposure);
//    var totalRequired = items.Sum(i => i.RequiredCollateral);

//    var response = new CalculateCollateralResponse(
//        CustomerId: req.CustomerId,
//        TotalExposure: totalExposure,
//        TotalRequiredCollateral: totalRequired,
//        Items: items
//    );

//    return Results.Ok(response);
//})
//.WithName("CalculateCollateral");

//app.Run();


//record CalculateCollateralRequest(Guid CustomerId);

//record CalculateCollateralResponse(
//    Guid CustomerId,
//    decimal TotalExposure,
//    decimal TotalRequiredCollateral,
//    List<CollateralItem> Items
//);

//record CollateralItem(
//    string Symbol,
//    string AssetType,
//    decimal Exposure,
//    decimal Haircut,
//    decimal RequiredCollateral,
//    string Currency
//);

//record PositionDto(
//    Guid CustomerId,
//    string Symbol,
//    string AssetType,
//    decimal Quantity,
//    decimal Price,
//    string Currency,
//    DateTimeOffset UpdatedAt
//);