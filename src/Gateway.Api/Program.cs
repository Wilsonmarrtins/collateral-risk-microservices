using Microsoft.Extensions.ServiceDiscovery; // INCLUÍDO: necessário para Service Discovery
using Microsoft.Extensions.ServiceDiscovery.Yarp; // INCLUÍDO: integração do Service Discovery com YARP

var builder = WebApplication.CreateBuilder(args);

// ==============================
// SERVICE DISCOVERY (ASPIRE)
// ==============================

// INCLUÍDO: registra o mecanismo de Service Discovery.
// Isso permite que o Gateway descubra serviços como:
// https://customers-api
// https://positions-api
// sem precisar de localhost + porta
builder.Services.AddServiceDiscovery();


// ==============================
// YARP - REVERSE PROXY
// ==============================

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    // INCLUÍDO: permite que o YARP resolva destinos usando Service Discovery
    // Exemplo: "https://customers-api"
    .AddServiceDiscoveryDestinationResolver();


// ==============================
// SWAGGER
// ==============================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// ==============================
// SWAGGER UI
// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        // ALTERADO: removidas portas fixas
        // Agora usamos os nomes dos serviços registrados no Aspire

        c.SwaggerEndpoint("https://customers-api/swagger/v1/swagger.json", "Customers API");

        c.SwaggerEndpoint("https://positions-api/swagger/v1/swagger.json", "Positions API");

        c.SwaggerEndpoint("https://collateral-api/swagger/v1/swagger.json", "Collateral API");

        // Mantido
        c.RoutePrefix = "swagger";
    });
}


// ==============================
// REVERSE PROXY
// ==============================

// Mantido: YARP intercepta as rotas configuradas no appsettings
app.MapReverseProxy();

app.Run();