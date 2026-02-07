-- =========================================================================================
-- SCRIPT DE INICIALIZACIÓN PARA BASE DE DATOS NUEVA (site4now.net)
-- FECHA: 2026-02-03
-- BASE DE DATOS: db_ac4b7f_orcinversiones
-- DESCRIPCIÓN: Crea todas las tablas necesarias y el usuario admin inicial
-- =========================================================================================

-- 1. CREAR TABLA ROLES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](200) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [dbo].[Roles]([Name]);
    PRINT 'Tabla Roles creada.';
END

-- 2. CREAR TABLA USERS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](50) NOT NULL,
        [Email] [nvarchar](100) NOT NULL,
        [PasswordHash] [nvarchar](255) NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime2](7) NULL,
        [RoleId] [int] NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY([RoleId]) REFERENCES [dbo].[Roles] ([Id])
    );
    CREATE UNIQUE INDEX [IX_Users_Username] ON [dbo].[Users]([Username]);
    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
    CREATE INDEX [IX_Users_RoleId] ON [dbo].[Users]([RoleId]);
    PRINT 'Tabla Users creada.';
END

-- 3. CREAR TABLA CATEGORIES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NOT NULL DEFAULT '',
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE UNIQUE INDEX [IX_Categories_Name] ON [dbo].[Categories]([Name]);
    PRINT 'Tabla Categories creada.';
END

-- 4. CREAR TABLA NOMBREMARCAS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NombreMarcas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NombreMarcas](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_NombreMarcas] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE UNIQUE INDEX [IX_NombreMarcas_Nombre] ON [dbo].[NombreMarcas]([Nombre]);
    PRINT 'Tabla NombreMarcas creada.';
END

-- 5. CREAR TABLA PRODUCTS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Codigo] [nvarchar](50) NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](1000) NOT NULL DEFAULT '',
        [DescripcionFichaTecnica] [nvarchar](max) NULL,
        [Producto] [nvarchar](200) NULL,
        [Price] [decimal](18, 2) NOT NULL,
        [Stock] [int] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime2](7) NULL,
        [CategoryId] [int] NOT NULL,
        [MarcaId] [int] NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
        CONSTRAINT [FK_Products_NombreMarcas_MarcaId] FOREIGN KEY([MarcaId]) REFERENCES [dbo].[NombreMarcas] ([Id])
    );
    CREATE INDEX [IX_Products_CategoryId] ON [dbo].[Products]([CategoryId]);
    CREATE INDEX [IX_Products_MarcaId] ON [dbo].[Products]([MarcaId]);
    CREATE INDEX [IX_Products_Name] ON [dbo].[Products]([Name]);
    CREATE INDEX [IX_Products_Codigo] ON [dbo].[Products]([Codigo]);
    PRINT 'Tabla Products creada.';
END

-- 6. CREAR TABLA PRODUCTIMAGES
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductImages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductImages](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ProductId] [int] NOT NULL,
        [ImageUrl] [nvarchar](max) NOT NULL,
        [OrderIndex] [int] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ProductImages] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages]([ProductId]);
    CREATE INDEX [IX_ProductImages_ProductId_OrderIndex] ON [dbo].[ProductImages]([ProductId], [OrderIndex]);
    PRINT 'Tabla ProductImages creada.';
END

-- 7. CREAR TABLA __EFMigrationsHistory (para EF Core)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory](
        [MigrationId] [nvarchar](150) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
    );
    PRINT 'Tabla __EFMigrationsHistory creada.';
END

-- =========================================================================================
-- INSERTAR DATOS INICIALES
-- =========================================================================================

-- Insertar Roles si no existen
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Administrador')
BEGIN
    SET IDENTITY_INSERT [dbo].[Roles] ON;
    INSERT INTO [dbo].[Roles] ([Id], [Name], [Description], [CreatedAt])
    VALUES (1, 'Administrador', 'Acceso total al sistema', GETDATE());
    SET IDENTITY_INSERT [dbo].[Roles] OFF;
    PRINT 'Rol Administrador creado.';
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Vendedor')
BEGIN
    SET IDENTITY_INSERT [dbo].[Roles] ON;
    INSERT INTO [dbo].[Roles] ([Id], [Name], [Description], [CreatedAt])
    VALUES (2, 'Vendedor', 'Acceso restringido a productos', GETDATE());
    SET IDENTITY_INSERT [dbo].[Roles] OFF;
    PRINT 'Rol Vendedor creado.';
END

-- Insertar Usuario Admin si no existe
-- Contraseña: Admin123! (hash BCrypt)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [PasswordHash], [IsActive], [RoleId], [CreatedAt])
    VALUES ('admin', 'admin@orcinversiones.com', '$2a$11$coMcJF6JoQYOAbzXPavCveSDeNIZ8yeIvhuhUBAIngZyUKdD9cFCa', 1, 1, GETDATE());
    PRINT 'Usuario admin creado. Contraseña: Admin123!';
END

-- Insertar Categoría de ejemplo
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'General')
BEGIN
    INSERT INTO [dbo].[Categories] ([Name], [Description], [IsActive], [CreatedAt])
    VALUES ('General', 'Categoría general de productos', 1, GETDATE());
    PRINT 'Categoría General creada.';
END

-- Registrar migraciones como aplicadas
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260122214514_InitialCreate')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260122214514_InitialCreate', '8.0.0');
END

PRINT '';
PRINT '=========================================================================================';
PRINT 'INICIALIZACIÓN COMPLETADA';
PRINT '';
PRINT 'Usuario Admin:';
PRINT '  - Username: admin';
PRINT '  - Password: Admin123!';
PRINT '  - Email: admin@orcinversiones.com';
PRINT '=========================================================================================';
