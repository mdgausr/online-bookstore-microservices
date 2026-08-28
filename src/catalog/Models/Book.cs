namespace Catalog.Models;

public record Book
(
    int Id,
    string Title,
    string Author,
    decimal Price,
    string Description,
    int Stock
);

public record Review(string Author, int Rating, string Comment);
