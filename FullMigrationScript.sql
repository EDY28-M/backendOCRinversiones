-- =========================================================================================
-- SCRIPT COMPLETO DE REFACTORIZACIÓN DE IMÁGENES
-- FECHA: 2026-02-01
-- DESCRIPCIÓN:
-- 1. Crea la tabla ProductImages.
-- 2. Migra los datos de las columnas antiguas (ImagenPrincipal, Imagen2, etc.).
-- 3. Limpia la tabla Products (Elimina las columnas antiguas).
-- =========================================================================================

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. CREAR TABLA ProductImages SI NO EXISTE
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductImages]') AND type in (N'U'))
    BEGIN
        PRINT 'Creando tabla ProductImages...';
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

        -- Crear índices para optimizar búsquedas por Producto y Orden
        CREATE INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages]([ProductId]);
        CREATE INDEX [IX_ProductImages_ProductId_OrderIndex] ON [dbo].[ProductImages]([ProductId], [OrderIndex]);
        
        PRINT 'Tabla ProductImages creada exitosamente.';
    END
    ELSE
    BEGIN
        PRINT 'La tabla ProductImages ya existe.';
    END

    -- 2. MIGRACIÓN DE DATOS (Idempotente: No duplica si ya se corrió)
    PRINT 'Iniciando migración de imágenes...';

    -- Migrar ImagenPrincipal (OrderIndex = 0)
    INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
    SELECT Id, ImagenPrincipal, 0, 1, GETUTCDATE()
    FROM Products
    WHERE ImagenPrincipal IS NOT NULL 
      AND ImagenPrincipal <> ''
      AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 0);

    -- Migrar Imagen2 (OrderIndex = 1)
    INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
    SELECT Id, Imagen2, 1, 1, GETUTCDATE()
    FROM Products
    WHERE Imagen2 IS NOT NULL 
      AND Imagen2 <> ''
      AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 1);

    -- Migrar Imagen3 (OrderIndex = 2)
    INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
    SELECT Id, Imagen3, 2, 1, GETUTCDATE()
    FROM Products
    WHERE Imagen3 IS NOT NULL 
      AND Imagen3 <> ''
      AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 2);

    -- Migrar Imagen4 (OrderIndex = 3)
    INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
    SELECT Id, Imagen4, 3, 1, GETUTCDATE()
    FROM Products
    WHERE Imagen4 IS NOT NULL 
      AND Imagen4 <> ''
      AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 3);

    PRINT 'Migración de datos completada.';

    -- 3. ELIMINAR COLUMNAS ANTIGUAS
    -- IMPORTANTE: Solo descomentar esta sección si estás 100% seguro de que los datos se migraron bien.
    -- Se recomienda correr esto en un paso posterior tras verificar visualmente la tabla ProductImages.
    
    /*
    PRINT 'Eliminando columnas antiguas de la tabla Products...';
    
    IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ImagenPrincipal' AND Object_ID = Object_ID(N'dbo.Products'))
        ALTER TABLE Products DROP COLUMN ImagenPrincipal;

    IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Imagen2' AND Object_ID = Object_ID(N'dbo.Products'))
        ALTER TABLE Products DROP COLUMN Imagen2;

    IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Imagen3' AND Object_ID = Object_ID(N'dbo.Products'))
        ALTER TABLE Products DROP COLUMN Imagen3;

    IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Imagen4' AND Object_ID = Object_ID(N'dbo.Products'))
        ALTER TABLE Products DROP COLUMN Imagen4;
        
    PRINT 'Columnas eliminadas exitosamente.';
    */
    
    COMMIT TRANSACTION;
    PRINT 'Refactorización completada con éxito.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error detectado. Se ha revertido la transacción.';
    PRINT ERROR_MESSAGE();
END CATCH;
