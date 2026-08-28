using System.Data;
using Dapper;
using SharedMessages;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace Orders.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(Guid userId, decimal total);
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

    public async Task<Guid> CreateOrderAsync(Guid userId, decimal total)
    {
        var orderId = Guid.NewGuid();
        var sql = "INSERT INTO Orders (Id, UserId, Total, CreatedAt) VALUES (@Id, @UserId, @Total, GETUTCDATE())";
        await _db.ExecuteAsync(sql, new { Id = orderId, UserId = userId, Total = total });

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
