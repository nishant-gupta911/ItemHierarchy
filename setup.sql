CREATE DATABASE ItemProcessingDB;
GO

USE ItemProcessingDB;
GO

CREATE TABLE Items (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Weight FLOAT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE ItemRelations (
    RelationId INT PRIMARY KEY IDENTITY(1,1),
    ParentItemId INT NOT NULL,
    ChildItemId INT NOT NULL,

    FOREIGN KEY (ParentItemId) REFERENCES Items(ItemId),
    FOREIGN KEY (ChildItemId) REFERENCES Items(ItemId)
);

SELECT * FROM Items;