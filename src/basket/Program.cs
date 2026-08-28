using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.OpenApi.Models;
using Basket.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket API", Version = "v1" }));

var connectionString = builder.Configuration.GetConnectionString("Default") ?? builder.Configuration["ConnectionStrings:Default"] ?? "Server=mssql,1433;Database=basketdb;User Id=sa;Password=P@ssw0rd!";
builder.Services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));
builder.Services.AddScoped<IBasketRepository, BasketRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/health", () => Results.Ok(new { status = "Basket OK" }));

app.MapPost("/api/basket/{userId}/items", async (Guid userId, BasketItemCreate item, IBasketRepository repo) =>
{
    await repo.AddItemAsync(userId, new BasketItem(0, userId, (int)item.BookId, item.Quantity));
    return Results.Accepted();
});

app.MapGet("/api/basket/{userId}", async (Guid userId, IBasketRepository repo) =>
{
    var items = await repo.GetItemsAsync(userId);
    return Results.Ok(items);
});

app.MapPost("/api/basket/{userId}/clear", async (Guid userId, IBasketRepository repo) =>
{
    await repo.ClearAsync(userId);
    return Results.Ok();
});

app.Run();

record BasketItemCreate(Guid BookId, int Quantity);
