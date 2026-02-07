-- =====================================================
-- Script para asignar 4 imágenes a cada producto
-- Total: 2027 productos con 4 campos de imagen cada uno
-- Los 213 enlaces se reciclan automáticamente
-- =====================================================

-- Limpiar tabla temporal si ya existe (por ejecuciones previas)
IF OBJECT_ID('tempdb..#ImageUrls') IS NOT NULL
    DROP TABLE #ImageUrls;

-- Crear tabla temporal con todos los enlaces
CREATE TABLE #ImageUrls (
    RowNum INT IDENTITY(1,1),
    ImageUrl NVARCHAR(MAX)
);

-- Insertar todos los enlaces disponibles (213 total)
INSERT INTO #ImageUrls (ImageUrl) VALUES
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(6).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(7).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(8).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(9).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(12).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(13).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(40).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(41).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(64).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(65).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(66).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(67).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(68).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(69).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(70).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(71).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(72).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(73).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(74).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(75).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(76).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(77).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(78).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(79).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(80).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(81).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(82).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(83).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(84).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(85).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(86).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(87).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(88).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(89).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(90).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(91).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(92).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(93).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(94).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(95).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(96).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(97).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(98).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(99).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(110).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(111).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(112).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(113).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(114).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(115).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(116).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(117).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(118).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(119).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(120).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(121).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(122).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(123).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(124).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(125).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(126).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(127).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(128).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(129).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(131).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(132).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(133).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(139).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(140).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(141).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(143).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(144).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(145).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(146).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(147).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(148).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(149).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(150).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(151).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(152).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(153).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(154).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(155).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Auto+(156).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(3).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(4).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(5).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(6).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(7).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(9).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(12).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(13).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(15).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(28).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(29).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(30).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(31).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(32).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(33).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(34).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(35).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(36).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(37).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(38).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(39).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(40).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(41).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(42).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(43).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(44).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(45).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(46).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(47).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(51).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(52).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(53).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(54).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(55).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(56).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(57).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(58).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(59).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(60).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(61).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(62).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(63).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(64).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(65).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(66).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(67).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(68).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(69).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(70).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(71).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(72).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(73).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(74).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(75).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(76).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(77).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(78).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(79).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(80).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(81).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(82).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(83).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(84).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(85).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(86).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(87).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(88).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(89).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(90).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(91).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(92).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(93).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(94).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(95).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(96).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(97).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(98).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(99).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(110).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(111).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(112).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(113).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(114).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(115).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(116).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(117).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(118).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(119).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(120).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(121).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(122).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(123).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(124).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(125).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(126).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(127).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(128).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(129).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(133).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(134).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(135).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(136).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(137).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(138).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(139).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(140).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(141).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(142).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(143).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(144).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(145).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(146).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(147).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(148).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(149).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(150).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(151).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(152).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(153).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(154).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(155).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(156).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(157).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(158).jpg'),
('https://f005.backblazeb2.com/file/Images154/Datacluster+Truck+(159).jpg');

-- Verificar cuántos enlaces tenemos
DECLARE @TotalImages INT = (SELECT COUNT(*) FROM #ImageUrls);
PRINT 'Total de enlaces disponibles: ' + CAST(@TotalImages AS VARCHAR(10));

-- Actualizar los 4 campos de imagen de cada producto
UPDATE p
SET 
    ImagenPrincipal = img1.ImageUrl,
    Imagen2 = img2.ImageUrl,
    Imagen3 = img3.ImageUrl,
    Imagen4 = img4.ImageUrl,
    UpdatedAt = GETUTCDATE()
FROM Products p
CROSS APPLY (
    -- Imagen Principal (0)
    SELECT ImageUrl FROM #ImageUrls 
    WHERE RowNum = ((p.Id * 4 + 0) % @TotalImages) + 1
) img1
CROSS APPLY (
    -- Imagen 2 (1)
    SELECT ImageUrl FROM #ImageUrls 
    WHERE RowNum = ((p.Id * 4 + 1) % @TotalImages) + 1
) img2
CROSS APPLY (
    -- Imagen 3 (2)
    SELECT ImageUrl FROM #ImageUrls 
    WHERE RowNum = ((p.Id * 4 + 2) % @TotalImages) + 1
) img3
CROSS APPLY (
    -- Imagen 4 (3)
    SELECT ImageUrl FROM #ImageUrls 
    WHERE RowNum = ((p.Id * 4 + 3) % @TotalImages) + 1
) img4;

-- Verificar resultados
PRINT '=================================================';
PRINT 'Proceso completado!';
PRINT '=================================================';

SELECT 
    COUNT(*) AS TotalProductosActualizados,
    COUNT(CASE WHEN ImagenPrincipal IS NOT NULL THEN 1 END) AS ConImagenPrincipal,
    COUNT(CASE WHEN Imagen2 IS NOT NULL THEN 1 END) AS ConImagen2,
    COUNT(CASE WHEN Imagen3 IS NOT NULL THEN 1 END) AS ConImagen3,
    COUNT(CASE WHEN Imagen4 IS NOT NULL THEN 1 END) AS ConImagen4
FROM Products;

-- Ver ejemplos de productos actualizados (primeros 5)
SELECT TOP 5
    Id,
    Producto,
    LEFT(ImagenPrincipal, 80) AS ImagenPrincipal,
    LEFT(Imagen2, 80) AS Imagen2,
    LEFT(Imagen3, 80) AS Imagen3,
    LEFT(Imagen4, 80) AS Imagen4
FROM Products
ORDER BY Id;

-- Limpiar tabla temporal
DROP TABLE #ImageUrls;

PRINT 'Script ejecutado exitosamente!';
PRINT 'Cada producto ahora tiene 4 imágenes asignadas en sus columnas: ImagenPrincipal, Imagen2, Imagen3, Imagen4';
PRINT 'Los 213 enlaces se reciclaron automáticamente para cubrir todos los productos';
