using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);


var customers = builder.AddProject<Projects.Customers_Api>("customers-api");
var positions = builder.AddProject<Projects.Positions_Api>("positions-api");
var collateral = builder.AddProject<Projects.Collateral_Api>("collateral-api");
var marginTransfer = builder.AddProject<Projects.MarginTransfer_Api>("margintransfer-api");


var gateway = builder.AddProject<Projects.Gateway_Api>("gateway-api");



collateral.WithEnvironment(
    "Services__PositionsBaseUrl",
    positions.GetEndpoint("https")
);



gateway.WithReference(customers);
gateway.WithReference(positions);
gateway.WithReference(collateral);
gateway.WithReference(marginTransfer);



builder.Build().Run();