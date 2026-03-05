using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.Extensions.ServiceDiscovery.Yarp;

var builder = WebApplication.CreateBuilder(args);

// SERVICE DISCOVERY (ASPIRE)
builder.Services.AddServiceDiscovery();

// YARP - REVERSE PROXY
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// SWAGGER UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("https://customers-api/swagger/v1/swagger.json", "Customers API");
        c.SwaggerEndpoint("https://positions-api/swagger/v1/swagger.json", "Positions API");
        c.SwaggerEndpoint("https://collateral-api/swagger/v1/swagger.json", "Collateral API");

        // ✅ NOVO: Margin Transfer
        c.SwaggerEndpoint("https://margintransfer-api/swagger/v1/swagger.json", "MarginTransfer API");

        c.RoutePrefix = "swagger";
    });
}

// REVERSE PROXY
app.MapReverseProxy();

app.Run();