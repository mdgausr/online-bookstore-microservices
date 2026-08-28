using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using SharedMessages;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payments API", Version = "v1" }));

var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? builder.Configuration["RabbitMQ__Host"] ?? "rabbitmq";
var stripeKey = builder.Configuration["STRIPE__SecretKey"] ?? "__STRIPE_SECRET_PLACEHOLDER__";
StripeConfiguration.ApiKey = stripeKey;

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/health", () => Results.Ok(new { status = "Payments OK" }));

app.MapPost("/api/payments/charge", async (PaymentModel model) =>
{
    // Minimal Stripe charge simulation - in production call Stripe's PaymentIntent API
    // Here we return a simulated successful response
    return Results.Ok(new { success = true, charged = model.Amount });
});

// Subscribe to order.created events and attempt to reserve inventory or charge
_ = Task.Run(() =>
{
    try
    {
        var factory = new ConnectionFactory() { HostName = rabbitHost };
        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();
        channel.ExchangeDeclare("orders-exchange", "topic", durable: true);
        var qName = channel.QueueDeclare().QueueName;
        channel.QueueBind(qName, "orders-exchange", "order.created");
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var text = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<OrderCreated>(text);
            Console.WriteLine($"[Payments] Received OrderCreated: OrderId={order?.OrderId} Total={order?.Total}");
            // Here we would interact with Stripe and publish PaymentProcessed message
        };
        channel.BasicConsume(queue: qName, autoAck: true, consumer: consumer);
        while (true) Thread.Sleep(1000);
    }
    catch (Exception ex)
    {
        Console.WriteLine("[Payments] Subscriber setup failed: " + ex.Message);
    }
});

app.Run();

record PaymentModel(Guid OrderId, decimal Amount);
