using Customers.Api.Endpoints;
using Customers.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7000",
                "http://localhost:5000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<CustomerService>();

var app = builder.Build();

app.UseCors("AllowGateway");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customers API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapCustomersEndpoints();

app.Run();

//using System.Collections.Concurrent;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowGateway", policy =>
//    {
//        policy
//            .WithOrigins(
//                "https://localhost:7000",
//                "http://localhost:5000" 
//            )
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});

//var app = builder.Build();

//app.UseCors("AllowGateway");

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Customers API v1");
//        options.RoutePrefix = "swagger";
//    });
//}

//app.UseHttpsRedirection();

//var customersDb = new ConcurrentDictionary<Guid, Customer>();

//var customers = app.MapGroup("/v1/customers")
//    .WithTags("Customers");


//customers.MapPost("/", (CreateCustomerRequest req) =>
//{
//    if (string.IsNullOrWhiteSpace(req.Name))
//        return Results.BadRequest(new { error = "Name is required." });

//    if (string.IsNullOrWhiteSpace(req.Document))
//        return Results.BadRequest(new { error = "Document is required." });

//    var id = Guid.NewGuid();

//    var customer = new Customer(
//        Id: id,
//        Name: req.Name.Trim(),
//        Document: req.Document.Trim(),
//        CreatedAt: DateTimeOffset.UtcNow
//    );

//    customersDb[id] = customer;

//    return Results.Created($"/v1/customers/{id}", new { id });
//})
//.WithName("CreateCustomer");


//customers.MapGet("/{id:guid}", (Guid id) =>
//{
//    return customersDb.TryGetValue(id, out var customer)
//        ? Results.Ok(customer)
//        : Results.NotFound();
//})
//.WithName("GetCustomerById");


//customers.MapGet("/", () =>
//{
//    var list = customersDb.Values
//        .OrderByDescending(c => c.CreatedAt)
//        .ToList();

//    return Results.Ok(list);
//})
//.WithName("ListCustomers");

//app.Run();

//record CreateCustomerRequest(string Name, string Document);

//record Customer(
//    Guid Id,
//    string Name,
//    string Document,
//    DateTimeOffset CreatedAt
//);