using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.OpenApi.Models;
using Orders.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders API", Version = "v1" }));

var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["ConnectionStrings:Default"] ?? "Server=mssql,1433;Database=ordersdb;User Id=sa;Password=P@ssw0rd!";
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? builder.Configuration["RabbitMQ__Host"] ?? "rabbitmq";

builder.Services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));
builder.Services.AddScoped<IOrderService>(_ => new OrderService(new SqlConnection(connectionString), rabbitHost));

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/health", () => Results.Ok(new { status = "Orders OK" }));

app.MapPost("/api/orders", async (CreateOrderModel model, IOrderService service) =>
{
    // Basic idempotency: client can pass idempotency key in model (omitted for brevity)
    var orderId = await service.CreateOrderAsync(model.UserId, model.Total);
    return Results.Created($"/api/orders/{orderId}", new { orderId });
});

app.Run();

record CreateOrderModel(Guid UserId, decimal Total);
