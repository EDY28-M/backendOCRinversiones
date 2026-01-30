-- ============================================
-- MIGRACIÓN MANUAL: Índices de Optimización
-- Backend OCR Inversiones
-- Fecha: 2026-01-30
-- ============================================

USE [ORCInversiones_Dev]
GO

PRINT '=========================================='
PRINT 'INICIANDO MIGRACIÓN DE ÍNDICES OPTIMIZADOS'
PRINT '=========================================='
GO

-- ============================================
-- 1. RECREAR ÍNDICES EN Products.Codigo Y Products.CodigoComer COMO ÚNICOS
-- ============================================
PRINT 'Paso 1: Verificando y recreando índices únicos en Codigo y CodigoComer...'
GO

-- Eliminar índice existente en Codigo si existe
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Codigo' AND object_id = OBJECT_ID('Products'))
BEGIN
    DROP INDEX IX_Products_Codigo ON Products;
    PRINT '  ✓ Índice IX_Products_Codigo eliminado';
END

-- Crear índice ÚNICO en Codigo
CREATE UNIQUE NONCLUSTERED INDEX IX_Products_Codigo
ON Products(Codigo)
INCLUDE (CodigoComer, Producto, IsActive)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF);
PRINT '  ✓ Índice ÚNICO IX_Products_Codigo creado';
GO

-- Eliminar índice existente en CodigoComer si existe
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CodigoComer' AND object_id = OBJECT_ID('Products'))
BEGIN
    DROP INDEX IX_Products_CodigoComer ON Products;
    PRINT '  ✓ Índice IX_Products_CodigoComer eliminado';
END

-- Crear índice ÚNICO en CodigoComer
CREATE UNIQUE NONCLUSTERED INDEX IX_Products_CodigoComer
ON Products(CodigoComer)
INCLUDE (Codigo, Producto, IsActive)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF);
PRINT '  ✓ Índice ÚNICO IX_Products_CodigoComer creado';
GO

-- ============================================
-- 2. ÍNDICES COMPUESTOS PARA FILTROS COMUNES
-- ============================================
PRINT 'Paso 2: Creando índices compuestos para filtros...'
GO

-- Índice compuesto para filtros por categoría activa
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsActive_CategoryId' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_CategoryId
    ON Products(IsActive, CategoryId)
    INCLUDE (Codigo, CodigoComer, Producto, CreatedAt, MarcaId, ImagenPrincipal, Imagen2, Imagen3, Imagen4)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, ONLINE = OFF);
    PRINT '  ✓ Índice IX_Products_IsActive_CategoryId creado';
END
ELSE
    PRINT '  - Índice IX_Products_IsActive_CategoryId ya existe';
GO

-- Índice compuesto para filtros por marca activa
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsActive_MarcaId' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_MarcaId
    ON Products(IsActive, MarcaId)
    INCLUDE (Codigo, CodigoComer, Producto, CreatedAt, CategoryId, ImagenPrincipal, Imagen2, Imagen3, Imagen4)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, ONLINE = OFF);
    PRINT '  ✓ Índice IX_Products_IsActive_MarcaId creado';
END
ELSE
    PRINT '  - Índice IX_Products_IsActive_MarcaId ya existe';
GO

-- ============================================
-- 3. ÍNDICES DE ORDENAMIENTO
-- ============================================
PRINT 'Paso 3: Creando índices de ordenamiento...'
GO

-- Índice para ordenamiento por fecha
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CreatedAt' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_CreatedAt
    ON Products(CreatedAt DESC)
    INCLUDE (Id, Codigo, CodigoComer, Producto, IsActive, CategoryId, MarcaId)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, ONLINE = OFF);
    PRINT '  ✓ Índice IX_Products_CreatedAt creado';
END
ELSE
    PRINT '  - Índice IX_Products_CreatedAt ya existe';
GO

-- Índice compuesto para búsquedas públicas (IsActive + CreatedAt)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsActive_CreatedAt' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_CreatedAt
    ON Products(IsActive, CreatedAt DESC)
    INCLUDE (Id, Codigo, CodigoComer, Producto, CategoryId, MarcaId, ImagenPrincipal)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, ONLINE = OFF);
    PRINT '  ✓ Índice IX_Products_IsActive_CreatedAt creado';
END
ELSE
    PRINT '  - Índice IX_Products_IsActive_CreatedAt ya existe';
GO

-- ============================================
-- 4. ACTUALIZAR ESTADÍSTICAS
-- ============================================
PRINT 'Paso 4: Actualizando estadísticas de la tabla Products...'
GO

UPDATE STATISTICS Products WITH FULLSCAN;
PRINT '  ✓ Estadísticas actualizadas';
GO

-- ============================================
-- 5. VERIFICACIÓN DE ÍNDICES CREADOS
-- ============================================
PRINT '=========================================='
PRINT 'VERIFICACIÓN DE ÍNDICES CREADOS:'
PRINT '=========================================='
GO

SELECT
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    STUFF((
        SELECT ', ' + COL_NAME(ic.object_id, ic.column_id)
        FROM sys.index_columns ic
        WHERE ic.object_id = i.object_id
        AND ic.index_id = i.index_id
        AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') AS KeyColumns,
    STUFF((
        SELECT ', ' + COL_NAME(ic.object_id, ic.column_id)
        FROM sys.index_columns ic
        WHERE ic.object_id = i.object_id
        AND ic.index_id = i.index_id
        AND ic.is_included_column = 1
        FOR XML PATH('')
    ), 1, 2, '') AS IncludedColumns
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('Products')
AND i.name IS NOT NULL
ORDER BY i.name;
GO

PRINT '=========================================='
PRINT 'MIGRACIÓN COMPLETADA EXITOSAMENTE'
PRINT '=========================================='
GO

-- ============================================
-- ROLLBACK SCRIPT (EJECUTAR SOLO SI ES NECESARIO)
-- ============================================
/*
USE [ORCInversiones_Dev]
GO

PRINT 'ROLLBACK: Eliminando índices optimizados...'

DROP INDEX IF EXISTS IX_Products_Codigo ON Products;
DROP INDEX IF EXISTS IX_Products_CodigoComer ON Products;
DROP INDEX IF EXISTS IX_Products_IsActive_CategoryId ON Products;
DROP INDEX IF EXISTS IX_Products_IsActive_MarcaId ON Products;
DROP INDEX IF EXISTS IX_Products_CreatedAt ON Products;
DROP INDEX IF EXISTS IX_Products_IsActive_CreatedAt ON Products;

-- Recrear índices originales (no únicos)
CREATE NONCLUSTERED INDEX IX_Products_Codigo ON Products(Codigo);
CREATE NONCLUSTERED INDEX IX_Products_CodigoComer ON Products(CodigoComer);

PRINT 'ROLLBACK completado'
GO
*/
