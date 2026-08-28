using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.OpenApi.Models;
using Catalog.Repositories;
using Catalog.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" }));

var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["ConnectionStrings:Default"] ?? "Server=mssql,1433;Database=catalogdb;User Id=sa;Password=P@ssw0rd!";
builder.Services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));

// Register repository
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/health", () => Results.Ok(new { status = "Catalog OK" }));

app.MapGet("/api/books", async (IBookRepository repo) =>
{
    var books = await repo.GetAllAsync();
    return Results.Ok(books);
});

app.MapGet("/api/books/search", async (string q, IBookRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(q)) return Results.BadRequest("q is required");
    var results = await repo.SearchAsync(q);
    return Results.Ok(results);
});

app.MapGet("/api/books/{id}", async (int id, IBookRepository repo) =>
{
    var book = await repo.GetByIdAsync(id);
    if (book == null) return Results.NotFound();
    var promo = await repo.GetPromotionForBookAsync(id);
    return Results.Ok(new { book, promotion = promo });
});

app.MapGet("/api/books/{id}/reviews", async (int id, IBookRepository repo) =>
{
    var reviews = await repo.GetReviewsAsync(id);
    return Results.Ok(reviews);
});

app.MapPost("/api/books", async (BookCreateModel model, IBookRepository repo) =>
{
    var id = await repo.CreateAsync(new Book(0, model.Title, model.Author, model.Price, model.Description, model.Stock));
    return Results.Created($"/api/books/{id}", new { id });
});

app.MapPost("/api/books/{id}/reviews", async (int id, Review review, IBookRepository repo) =>
{
    await repo.AddReviewAsync(id, review);
    return Results.Ok();
});

app.Run();

record BookCreateModel(string Title, string Author, decimal Price, string Description, int Stock);
