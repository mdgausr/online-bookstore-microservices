-- SQL scripts to create databases and minimal tables
CREATE DATABASE catalogdb;
CREATE DATABASE basketdb;
CREATE DATABASE ordersdb;
CREATE DATABASE identitydb;

-- Use catalogdb for books
USE catalogdb;
CREATE TABLE Books (
  Id INT IDENTITY PRIMARY KEY,
  Title NVARCHAR(255),
  Author NVARCHAR(255),
  Price DECIMAL(18,2),
  Description NVARCHAR(MAX),
  Stock INT
);
CREATE TABLE Reviews (
  Id INT IDENTITY PRIMARY KEY,
  BookId INT,
  Author NVARCHAR(255),
  Rating INT,
  Comment NVARCHAR(MAX),
  CreatedAt DATETIME
);
CREATE TABLE Promotions (
  Id INT IDENTITY PRIMARY KEY,
  BookId INT,
  Description NVARCHAR(255),
  DiscountPercent INT,
  StartsAt DATETIME,
  EndsAt DATETIME
);
INSERT INTO Books (Title, Author, Price, Description, Stock) VALUES
('The Pragmatic Programmer', 'Andrew Hunt', 39.99, 'Classic software craftsmanship', 10),
('Clean Code', 'Robert C. Martin', 34.99, 'A Handbook of Agile Software Craftsmanship', 8);
INSERT INTO Promotions (BookId, Description, DiscountPercent, StartsAt, EndsAt) VALUES
(1, 'Summer sale', 10, GETDATE(), DATEADD(day,30,GETDATE()));

-- Basket DB
USE basketdb;
CREATE TABLE BasketItems (
  Id INT IDENTITY PRIMARY KEY,
  UserId UNIQUEIDENTIFIER,
  BookId INT,
  Quantity INT
);

-- Orders DB
USE ordersdb;
CREATE TABLE Orders (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  UserId UNIQUEIDENTIFIER,
  Total DECIMAL(18,2),
  CreatedAt DATETIME,
  Status NVARCHAR(50)
);
CREATE TABLE IdempotencyKeys (
  Id INT IDENTITY PRIMARY KEY,
  KeyValue NVARCHAR(255) UNIQUE,
  CreatedAt DATETIME
);
-- Projection table for quick reads
CREATE TABLE OrdersView (
  OrderId UNIQUEIDENTIFIER PRIMARY KEY,
  UserId UNIQUEIDENTIFIER,
  Total DECIMAL(18,2),
  Status NVARCHAR(50),
  UpdatedAt DATETIME
);

-- Identity DB
USE identitydb;
CREATE TABLE Users (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  Email NVARCHAR(255) UNIQUE,
  PasswordHash NVARCHAR(255)
);
-- Seed admin user: password P@ssw0rd!
INSERT INTO Users (Id, Email, PasswordHash) VALUES (NEWID(), 'admin@example.com', 'REPLACE_WITH_HASH_AFTER_SETUP');
