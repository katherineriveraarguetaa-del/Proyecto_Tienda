CREATE DATABASE TiendaDB;
GO

USE TiendaDB;
GO

CREATE TABLE Categorias (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Productos (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    CategoriaId INT NOT NULL,
    CONSTRAINT FK_Productos_Categorias
        FOREIGN KEY (CategoriaId)
        REFERENCES Categorias(Id)
        ON DELETE CASCADE
);
GO

INSERT INTO Categorias (Nombre)
VALUES (N'Electrónica'), (N'Ropa');
GO

INSERT INTO Productos (Nombre, Precio, CategoriaId)
VALUES
    (N'Laptop', 650.00, 1),
    (N'Teléfono', 250.00, 1),
    (N'Camisa', 15.00, 2);
GO