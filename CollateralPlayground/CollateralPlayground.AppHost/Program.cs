using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// APIs
var customers = builder.AddProject<Projects.Customers_Api>("customers-api");
var positions = builder.AddProject<Projects.Positions_Api>("positions-api");
var collateral = builder.AddProject<Projects.Collateral_Api>("collateral-api");

// Gateway
var gateway = builder.AddProject<Projects.Gateway_Api>("gateway-api");

// Collateral chama Positions
collateral.WithEnvironment("Services__PositionsBaseUrl", positions.GetEndpoint("https"));

// Ordem de subida
gateway.WithReference(customers);
gateway.WithReference(positions);
gateway.WithReference(collateral);

builder.Build().Run();