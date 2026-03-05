using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ============================
// APIs
// ============================

var customers = builder.AddProject<Projects.Customers_Api>("customers-api");

var positions = builder.AddProject<Projects.Positions_Api>("positions-api");

var collateral = builder.AddProject<Projects.Collateral_Api>("collateral-api");

var marginTransfer = builder.AddProject<Projects.MarginTransfer_Api>("margintransfer-api");


// ============================
// Gateway
// ============================

var gateway = builder.AddProject<Projects.Gateway_Api>("gateway-api");


// ============================
// Service communication
// ============================

// Collateral chama Positions
collateral.WithEnvironment(
    "Services__PositionsBaseUrl",
    positions.GetEndpoint("https")
);


// ============================
// Gateway references
// ============================

gateway.WithReference(customers);
gateway.WithReference(positions);
gateway.WithReference(collateral);
gateway.WithReference(marginTransfer);


// ============================
// Run
// ============================

builder.Build().Run();