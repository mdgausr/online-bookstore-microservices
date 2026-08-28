using Dapper;
using System.Data;

namespace Basket.Repositories;

public interface IBasketRepository
{
    Task AddItemAsync(Guid userId, BasketItem item);
    Task<IEnumerable<BasketItem>> GetItemsAsync(Guid userId);
    Task ClearAsync(Guid userId);
}

public class BasketRepository : IBasketRepository
{
    private readonly IDbConnection _db;
    public BasketRepository(IDbConnection db) => _db = db;

    public async Task AddItemAsync(Guid userId, BasketItem item)
    {
        var sql = "INSERT INTO BasketItems (UserId, BookId, Quantity) VALUES (@UserId, @BookId, @Quantity)";
        await _db.ExecuteAsync(sql, new { UserId = userId, item.BookId, item.Quantity });
    }

    public async Task<IEnumerable<BasketItem>> GetItemsAsync(Guid userId)
    {
        var sql = "SELECT Id, UserId, BookId, Quantity FROM BasketItems WHERE UserId = @UserId";
        return await _db.QueryAsync<BasketItem>(sql, new { UserId = userId });
    }

    public async Task ClearAsync(Guid userId)
    {
        var sql = "DELETE FROM BasketItems WHERE UserId = @UserId";
        await _db.ExecuteAsync(sql, new { UserId = userId });
    }
}

public record BasketItem(int Id, Guid UserId, int BookId, int Quantity);
