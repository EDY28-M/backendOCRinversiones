-- ============================================
-- Stored Procedures para Optimización de Performance
-- Backend OCR Inversiones
-- ============================================

USE [ORCInversiones_Dev]
GO

-- ============================================
-- SP 1: Búsqueda optimizada de productos con filtros
-- Reemplaza queries complejas con múltiples filtros
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetAvailableProductsPaged]
    @Page INT = 1,
    @PageSize INT = 12,
    @SearchTerm NVARCHAR(300) = NULL,
    @CategoryId INT = NULL,
    @OnlyActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Query optimizada con índices compuestos
    WITH FilteredProducts AS (
        SELECT
            p.Id,
            p.Codigo,
            p.CodigoComer,
            p.Producto,
            p.Descripcion,
            p.FichaTecnica,
            p.ImagenPrincipal,
            p.Imagen2,
            p.Imagen3,
            p.Imagen4,
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt,
            p.CategoryId,
            c.Name AS CategoryName,
            p.MarcaId,
            m.Nombre AS MarcaNombre
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        INNER JOIN NombreMarcas m ON p.MarcaId = m.Id
        WHERE
            (@OnlyActive = 0 OR p.IsActive = 1)
            AND (
                p.ImagenPrincipal IS NOT NULL AND p.ImagenPrincipal != '' OR
                p.Imagen2 IS NOT NULL AND p.Imagen2 != '' OR
                p.Imagen3 IS NOT NULL AND p.Imagen3 != '' OR
                p.Imagen4 IS NOT NULL AND p.Imagen4 != ''
            )
            AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
            AND (
                @SearchTerm IS NULL OR
                LOWER(p.Producto) LIKE '%' + LOWER(@SearchTerm) + '%' OR
                LOWER(p.Codigo) LIKE '%' + LOWER(@SearchTerm) + '%' OR
                LOWER(p.CodigoComer) LIKE '%' + LOWER(@SearchTerm) + '%' OR
                LOWER(c.Name) LIKE '%' + LOWER(@SearchTerm) + '%' OR
                LOWER(m.Nombre) LIKE '%' + LOWER(@SearchTerm) + '%'
            )
    )
    SELECT
        *,
        (SELECT COUNT(*) FROM FilteredProducts) AS TotalCount
    FROM FilteredProducts
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================
-- SP 2: Validación rápida de código disponible
-- Optimizada con índice único
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_IsCodigoAvailable]
    @Codigo NVARCHAR(100),
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM Products WITH (NOLOCK)
        WHERE UPPER(Codigo) = UPPER(@Codigo)
        AND (@ExcludeId IS NULL OR Id != @ExcludeId)
    )
        SELECT CAST(0 AS BIT) AS IsAvailable;
    ELSE
        SELECT CAST(1 AS BIT) AS IsAvailable;
END
GO

-- ============================================
-- SP 3: Obtener códigos para generación (optimizado)
-- Solo trae columnas necesarias
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetCodigosForGeneration]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Codigo,
        CodigoComer
    FROM Products WITH (NOLOCK);
END
GO

-- ============================================
-- SP 4: Estadísticas de productos por categoría/marca
-- Para dashboards y reportes rápidos
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetProductStatistics]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        'TotalProducts' AS Metric,
        COUNT(*) AS Value
    FROM Products
    UNION ALL
    SELECT
        'ActiveProducts',
        COUNT(*)
    FROM Products
    WHERE IsActive = 1
    UNION ALL
    SELECT
        'ProductsWithImages',
        COUNT(*)
    FROM Products
    WHERE IsActive = 1
    AND (
        ImagenPrincipal IS NOT NULL AND ImagenPrincipal != '' OR
        Imagen2 IS NOT NULL AND Imagen2 != '' OR
        Imagen3 IS NOT NULL AND Imagen3 != '' OR
        Imagen4 IS NOT NULL AND Imagen4 != ''
    )
    UNION ALL
    SELECT
        'TotalCategories',
        COUNT(DISTINCT CategoryId)
    FROM Products
    WHERE IsActive = 1
    UNION ALL
    SELECT
        'TotalBrands',
        COUNT(DISTINCT MarcaId)
    FROM Products
    WHERE IsActive = 1;
END
GO

-- ============================================
-- SP 5: Bulk Insert optimizado con validaciones
-- Para importaciones masivas
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_BulkInsertProducts]
    @ProductsJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Tabla temporal para staging
        CREATE TABLE #TempProducts (
            Codigo NVARCHAR(100),
            CodigoComer NVARCHAR(100),
            Producto NVARCHAR(300),
            Descripcion NVARCHAR(MAX),
            FichaTecnica NVARCHAR(MAX),
            CategoryId INT,
            MarcaId INT,
            ImagenPrincipal NVARCHAR(MAX),
            Imagen2 NVARCHAR(MAX),
            Imagen3 NVARCHAR(MAX),
            Imagen4 NVARCHAR(MAX)
        );

        -- Parsear JSON a tabla temporal
        INSERT INTO #TempProducts
        SELECT
            Codigo,
            CodigoComer,
            Producto,
            Descripcion,
            FichaTecnica,
            CategoryId,
            MarcaId,
            ImagenPrincipal,
            Imagen2,
            Imagen3,
            Imagen4
        FROM OPENJSON(@ProductsJson)
        WITH (
            Codigo NVARCHAR(100) '$.Codigo',
            CodigoComer NVARCHAR(100) '$.CodigoComer',
            Producto NVARCHAR(300) '$.Producto',
            Descripcion NVARCHAR(MAX) '$.Descripcion',
            FichaTecnica NVARCHAR(MAX) '$.FichaTecnica',
            CategoryId INT '$.CategoryId',
            MarcaId INT '$.MarcaId',
            ImagenPrincipal NVARCHAR(MAX) '$.ImagenPrincipal',
            Imagen2 NVARCHAR(MAX) '$.Imagen2',
            Imagen3 NVARCHAR(MAX) '$.Imagen3',
            Imagen4 NVARCHAR(MAX) '$.Imagen4'
        );

        -- Insertar solo productos que no existen (evita duplicados)
        INSERT INTO Products (
            Codigo,
            CodigoComer,
            Producto,
            Descripcion,
            FichaTecnica,
            CategoryId,
            MarcaId,
            ImagenPrincipal,
            Imagen2,
            Imagen3,
            Imagen4,
            IsActive,
            CreatedAt
        )
        SELECT
            t.Codigo,
            t.CodigoComer,
            t.Producto,
            t.Descripcion,
            t.FichaTecnica,
            t.CategoryId,
            t.MarcaId,
            t.ImagenPrincipal,
            t.Imagen2,
            t.Imagen3,
            t.Imagen4,
            1, -- IsActive
            GETDATE() -- CreatedAt
        FROM #TempProducts t
        WHERE NOT EXISTS (
            SELECT 1
            FROM Products p
            WHERE UPPER(p.Codigo) = UPPER(t.Codigo)
        );

        -- Retornar estadísticas
        SELECT
            @@ROWCOUNT AS InsertedCount,
            (SELECT COUNT(*) FROM #TempProducts) AS TotalSubmitted,
            (SELECT COUNT(*) FROM #TempProducts) - @@ROWCOUNT AS DuplicatesSkipped;

        DROP TABLE #TempProducts;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ============================================
-- Índices de texto completo (Full-Text Search)
-- Para búsquedas rápidas de texto
-- ============================================

-- Crear catálogo de texto completo si no existe
IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'ProductsCatalog')
BEGIN
    CREATE FULLTEXT CATALOG ProductsCatalog AS DEFAULT;
END
GO

-- Crear índice de texto completo en columna Producto
IF NOT EXISTS (
    SELECT 1
    FROM sys.fulltext_indexes fi
    INNER JOIN sys.tables t ON fi.object_id = t.object_id
    WHERE t.name = 'Products'
)
BEGIN
    CREATE FULLTEXT INDEX ON Products(Producto LANGUAGE 1033)
    KEY INDEX PK_Products
    ON ProductsCatalog
    WITH CHANGE_TRACKING AUTO;
END
GO

PRINT 'Stored Procedures y Full-Text Index creados exitosamente';
GO
