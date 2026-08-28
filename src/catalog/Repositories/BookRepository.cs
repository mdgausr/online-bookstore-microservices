using Dapper;
using System.Data;

namespace Catalog.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task<int> CreateAsync(Book book);
    Task AddReviewAsync(int bookId, Review review);
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
}
