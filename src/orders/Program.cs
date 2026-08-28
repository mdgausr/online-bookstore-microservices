using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.OpenApi.Models;
using Orders.Services;
using SharedMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;

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
    var orderId = await service.CreateOrderAsync(model.UserId, model.Total, model.IdempotencyKey);
    return Results.Created($"/api/orders/{orderId}", new { orderId });
});

// Background consumer: listen for payment processed events and update order status
_ = Task.Run(() =>
{
    try
    {
        var factory = new ConnectionFactory() { HostName = rabbitHost };
        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();
        channel.ExchangeDeclare("payments-exchange", "topic", durable: true);
        var qName = channel.QueueDeclare().QueueName;
        channel.QueueBind(qName, "payments-exchange", "payment.processed");
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var text = Encoding.UTF8.GetString(body);
            var evt = JsonSerializer.Deserialize<PaymentProcessed>(text);
            if (evt != null)
            {
                Console.WriteLine($"[Orders] PaymentProcessed received for OrderId={evt.OrderId} Success={evt.Success}");
                var status = evt.Success ? "Paid" : "PaymentFailed";
                using var db = new SqlConnection(connectionString);
                await db.ExecuteAsync("UPDATE Orders SET Status = @Status WHERE Id = @Id", new { Status = status, Id = evt.OrderId });
                await db.ExecuteAsync("UPDATE OrdersView SET Status = @Status, UpdatedAt = GETUTCDATE() WHERE OrderId = @Id", new { Status = status, Id = evt.OrderId });
            }
        };
        channel.BasicConsume(queue: qName, autoAck: true, consumer: consumer);
        while (true) Thread.Sleep(1000);
    }
    catch (Exception ex)
    {
        Console.WriteLine("[Orders] Payment consumer setup failed: " + ex.Message);
    }
});

app.Run();

record CreateOrderModel(Guid UserId, decimal Total, string? IdempotencyKey);
