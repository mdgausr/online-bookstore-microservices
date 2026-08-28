using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.OpenApi.Models;
using Catalog.Repositories;
using Catalog.Models;
using Basket.Repositories;
using Orders.Services;

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

app.MapGet("/api/books/{id}", async (int id, IBookRepository repo) =>
{
    var book = await repo.GetByIdAsync(id);
    return book is null ? Results.NotFound() : Results.Ok(book);
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
