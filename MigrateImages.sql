-- Script de Migración de Datos: Columnas Legacy -> Tabla ProductImages

-- 1. Migrar ImagenPrincipal (Orden 0)
INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
SELECT Id, ImagenPrincipal, 0, 1, GETUTCDATE()
FROM Products
WHERE ImagenPrincipal IS NOT NULL 
  AND ImagenPrincipal <> ''
  AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 0);

-- 2. Migrar Imagen2 (Orden 1)
INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
SELECT Id, Imagen2, 1, 1, GETUTCDATE()
FROM Products
WHERE Imagen2 IS NOT NULL 
  AND Imagen2 <> ''
  AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 1);

-- 3. Migrar Imagen3 (Orden 2)
INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
SELECT Id, Imagen3, 2, 1, GETUTCDATE()
FROM Products
WHERE Imagen3 IS NOT NULL 
  AND Imagen3 <> ''
  AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 2);

-- 4. Migrar Imagen4 (Orden 3)
INSERT INTO ProductImages (ProductId, ImageUrl, OrderIndex, IsActive, CreatedAt)
SELECT Id, Imagen4, 3, 1, GETUTCDATE()
FROM Products
WHERE Imagen4 IS NOT NULL 
  AND Imagen4 <> ''
  AND NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = Products.Id AND OrderIndex = 3);
