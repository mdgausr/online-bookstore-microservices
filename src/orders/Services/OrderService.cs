using Dapper;
using System.Data;
using SharedMessages;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace Orders.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(Guid userId, decimal total, string? idempotencyKey = null);
}

public class OrderService : IOrderService
{
    private readonly IDbConnection _db;
    private readonly string _rabbitHost;

    public OrderService(IDbConnection db, string rabbitHost)
    {
        _db = db;
        _rabbitHost = rabbitHost;
    }

    public async Task<Guid> CreateOrderAsync(Guid userId, decimal total, string? idempotencyKey = null)
    {
        // Idempotency check
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await _db.QueryFirstOrDefaultAsync<Guid?>("SELECT CAST(OrderId AS uniqueidentifier) FROM IdempotencyKeys WHERE KeyValue = @Key", new { Key = idempotencyKey });
            if (existing != null && existing != Guid.Empty) return existing.Value;
        }

        var orderId = Guid.NewGuid();
        var sql = "INSERT INTO Orders (Id, UserId, Total, CreatedAt, Status) VALUES (@Id, @UserId, @Total, GETUTCDATE(), @Status)";
        await _db.ExecuteAsync(sql, new { Id = orderId, UserId = userId, Total = total, Status = "Created" });

        // store idempotency mapping
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _db.ExecuteAsync("INSERT INTO IdempotencyKeys (KeyValue, CreatedAt) VALUES (@Key, GETUTCDATE())", new { Key = idempotencyKey });
        }

        // create projection
        await _db.ExecuteAsync("INSERT INTO OrdersView (OrderId, UserId, Total, Status, UpdatedAt) VALUES (@OrderId, @UserId, @Total, @Status, GETUTCDATE())", new { OrderId = orderId, UserId = userId, Total = total, Status = "Created" });

        // Publish event
        var factory = new ConnectionFactory() { HostName = _rabbitHost };
        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();
        channel.ExchangeDeclare("orders-exchange", "topic", durable: true);
        var message = new OrderCreated(orderId, userId, total);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange: "orders-exchange", routingKey: "order.created", body: body);

        return orderId;
    }
}
