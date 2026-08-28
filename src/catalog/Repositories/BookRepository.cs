using Dapper;
using System.Data;

namespace Catalog.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<int> CreateAsync(Book book);
    Task AddReviewAsync(int bookId, Review review);
    Task<IEnumerable<object>> SearchAsync(string q);
    Task<object?> GetPromotionForBookAsync(int bookId);
    Task<IEnumerable<object>> GetReviewsAsync(int bookId);
}

public class BookRepository : IBookRepository
{
    private readonly IDbConnection _db;
    public BookRepository(IDbConnection db) => _db = db;

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        var sql = "SELECT Id, Title, Author, Price, Description, Stock FROM Books";
        return await _db.QueryAsync<Book>(sql);
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        var sql = "SELECT Id, Title, Author, Price, Description, Stock FROM Books WHERE Id = @Id";
        return await _db.QueryFirstOrDefaultAsync<Book>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Book book)
    {
        var sql = "INSERT INTO Books (Title, Author, Price, Description, Stock) VALUES (@Title, @Author, @Price, @Description, @Stock); SELECT CAST(SCOPE_IDENTITY() as int)";
        return await _db.ExecuteScalarAsync<int>(sql, book);
    }

    public async Task AddReviewAsync(int bookId, Review review)
    {
        var sql = "INSERT INTO Reviews (BookId, Author, Rating, Comment, CreatedAt) VALUES (@BookId, @Author, @Rating, @Comment, GETUTCDATE())";
        await _db.ExecuteAsync(sql, new { BookId = bookId, review.Author, review.Rating, review.Comment });
    }

    public async Task<IEnumerable<object>> SearchAsync(string q)
    {
        var sql = "SELECT Id, Title, Author, Price, Description, Stock FROM Books WHERE Title LIKE @q OR Author LIKE @q OR Description LIKE @q";
        return (await _db.QueryAsync(sql, new { q = "%" + q + "%" })).Cast<object>();
    }

    public async Task<object?> GetPromotionForBookAsync(int bookId)
    {
        var sql = "SELECT Id, BookId, Description, DiscountPercent, StartsAt, EndsAt FROM Promotions WHERE BookId = @BookId AND StartsAt <= GETDATE() AND EndsAt >= GETDATE()";
        return await _db.QueryFirstOrDefaultAsync(sql, new { BookId = bookId });
    }

    public async Task<IEnumerable<object>> GetReviewsAsync(int bookId)
    {
        var sql = "SELECT Id, BookId, Author, Rating, Comment, CreatedAt FROM Reviews WHERE BookId = @BookId ORDER BY CreatedAt DESC";
        return await _db.QueryAsync(sql, new { BookId = bookId });
    }
}
