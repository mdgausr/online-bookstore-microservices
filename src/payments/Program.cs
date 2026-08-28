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

app.MapPost("/api/payments/create-payment-intent", async (CreatePaymentModel model) =>
{
    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)(model.Amount * 100),
        Currency = "usd",
        Metadata = new Dictionary<string, string>
        {
            { "orderId", model.OrderId.ToString() }
        }
    };
    var service = new PaymentIntentService();
    var pi = await service.CreateAsync(options);
    return Results.Ok(new { clientSecret = pi.ClientSecret, id = pi.Id });
});

// For demo/testing: confirm payment simulation endpoint that publishes PaymentProcessed
app.MapPost("/api/payments/simulate-confirm", (SimulateConfirmModel model) =>
{
    var factory = new ConnectionFactory() { HostName = rabbitHost };
    using var conn = factory.CreateConnection();
    using var channel = conn.CreateModel();
    channel.ExchangeDeclare("payments-exchange", "topic", durable: true);
    var message = new PaymentProcessed(model.OrderId, model.Success, model.PaymentIntentId);
    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    channel.BasicPublish(exchange: "payments-exchange", routingKey: "payment.processed", body: body);
    return Results.Ok(new { published = true });
});

// Keep an example subscriber to orders if needed
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
            // In a real app, you might create a PaymentIntent here or reserve funds
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

record CreatePaymentModel(Guid OrderId, decimal Amount);
record SimulateConfirmModel(Guid OrderId, bool Success, string? PaymentIntentId);
