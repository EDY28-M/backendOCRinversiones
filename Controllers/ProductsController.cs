using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Products;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Application.Constants;
using backendORCinverisones.Domain.Entities;
using backendORCinverisones.Infrastructure.Repositories;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly INombreMarcaRepository _nombreMarcaRepository;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ICacheService _cacheService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        INombreMarcaRepository nombreMarcaRepository,
        ICodeGeneratorService codeGeneratorService,
        ICacheService cacheService,
        IWebHostEnvironment env,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _nombreMarcaRepository = nombreMarcaRepository;
        _codeGeneratorService = codeGeneratorService;
        _cacheService = cacheService;
        _env = env;
        _logger = logger;
    }

    // ✅ SEGURIDAD: Helper para retornar errores sin exponer detalles en producción
    private ObjectResult SecureError(int statusCode, string message, Exception ex)
    {
        _logger.LogError(ex, message);
        var errorDetails = _env.IsDevelopment() ? ex.Message : "Ha ocurrido un error. Contacte al administrador.";
        return StatusCode(statusCode, new { message, error = errorDetails });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _productRepository.GetAllForListAsync();
        return Ok(response);
    }

    /// <summary>
    /// Obtiene productos disponibles (con al menos 1 imagen válida) con paginación
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? q = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool onlyWithImages = true)
    {
        try
        {
            var (items, total) = await _productRepository.GetAvailableProductsPagedAsync(
                page, pageSize, q, categoryId, onlyWithImages, onlyActive: true);

            var response = new PaginatedProductsResponseDto
            {
                Items = items.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    CodigoComer = p.CodigoComer,
                    Producto = p.Producto,
                    Descripcion = p.Descripcion,
                    FichaTecnica = p.FichaTecnica,
                    ImagenPrincipal = p.ImagenPrincipal,
                    Imagen2 = p.Imagen2,
                    Imagen3 = p.Imagen3,
                    Imagen4 = p.Imagen4,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    MarcaId = p.MarcaId,
                    MarcaNombre = p.Marca.Nombre
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener productos disponibles", ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);

        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        var response = new ProductResponseDto
        {
            Id = product.Id,
            Codigo = product.Codigo,
            CodigoComer = product.CodigoComer,
            Producto = product.Producto,
            Descripcion = product.Descripcion,
            FichaTecnica = product.FichaTecnica,
            ImagenPrincipal = product.ImagenPrincipal,
            Imagen2 = product.Imagen2,
            Imagen3 = product.Imagen3,
            Imagen4 = product.Imagen4,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            MarcaId = product.MarcaId,
            MarcaNombre = product.Marca.Nombre
        };

        return Ok(response);
    }

    /// <summary>
    /// Genera automáticamente el siguiente código y código comercial disponibles
    /// </summary>
    [HttpGet("generate-codes")]
    public async Task<IActionResult> GenerateCodes()
    {
        try
        {
            var response = await _codeGeneratorService.GenerateBothCodesAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al generar códigos automáticos", ex);
        }
    }

    /// <summary>
    /// Verifica si un código está disponible
    /// </summary>
    [HttpGet("check-codigo-available")]
    public async Task<IActionResult> CheckCodigoAvailable([FromQuery] string codigo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { message = "El código es requerido" });
                
            var isAvailable = await _codeGeneratorService.IsCodigoAvailableAsync(codigo);
            return Ok(new { codigo, isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del código");
            return StatusCode(500, new { message = "Error al verificar disponibilidad del código" });
        }
    }

    /// <summary>
    /// Verifica si un código comercial está disponible
    /// </summary>
    [HttpGet("check-codigo-comercial-available")]
    public async Task<IActionResult> CheckCodigoComercialAvailable([FromQuery] string codigoComer)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codigoComer))
                return BadRequest(new { message = "El código comercial es requerido" });
                
            var isAvailable = await _codeGeneratorService.IsCodigoComercialAvailableAsync(codigoComer);
            return Ok(new { codigoComer, isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad del código comercial");
            return StatusCode(500, new { message = "Error al verificar disponibilidad del código comercial" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Execute sequentially to avoid DbContext threading issues
            if (await _productRepository.IsCodigoExistsAsync(request.Codigo, null))
                return BadRequest(new { message = $"El código '{request.Codigo}' ya existe" });

            if (await _productRepository.IsCodigoComercialExistsAsync(request.CodigoComer, null))
                return BadRequest(new { message = $"El código comercial '{request.CodigoComer}' ya existe" });

            // Execute sequentially to avoid DbContext threading issues
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            var marca = await _nombreMarcaRepository.GetByIdAsync(request.MarcaId);

            if (category == null)
                return BadRequest(new { message = "La categoría especificada no existe" });
            if (marca == null)
                return BadRequest(new { message = "La marca especificada no existe" });

            var product = new Product
            {
                Codigo = request.Codigo.ToUpper(),
                CodigoComer = request.CodigoComer.ToUpper(),
                Producto = request.Producto,
                Descripcion = request.Descripcion,
                FichaTecnica = request.FichaTecnica,
                ImagenPrincipal = request.ImagenPrincipal,
                Imagen2 = request.Imagen2,
                Imagen3 = request.Imagen3,
                Imagen4 = request.Imagen4,
                CategoryId = request.CategoryId,
                MarcaId = request.MarcaId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsFeatured = request.IsFeatured
            };

            await _productRepository.AddAsync(product);
            _logger.LogInformation("Producto {Codigo} creado por {CurrentUser}", product.Codigo, User.Identity?.Name);

            _cacheService.RemoveByPrefix(CacheKeys.ProductsPrefix);

            // Respuesta desde entidad ya creada + category/marca ya cargados (evita GetByIdWithCategoryAsync)
            var response = new ProductResponseDto
            {
                Id = product.Id,
                Codigo = product.Codigo,
                CodigoComer = product.CodigoComer,
                Producto = product.Producto,
                Descripcion = product.Descripcion,
                FichaTecnica = product.FichaTecnica,
                ImagenPrincipal = product.ImagenPrincipal,
                Imagen2 = product.Imagen2,
                Imagen3 = product.Imagen3,
                Imagen4 = product.Imagen4,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                MarcaId = product.MarcaId,
                MarcaNombre = marca.Nombre
            };

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al crear el producto", ex);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            // Validar código si se está actualizando
            if (request.Codigo != null && request.Codigo.ToUpper() != product.Codigo.ToUpper())
            {
                var codigoExists = await _productRepository.IsCodigoExistsAsync(request.Codigo, id);

                if (codigoExists)
                    return BadRequest(new { message = $"El código '{request.Codigo}' ya existe" });

                product.Codigo = request.Codigo.ToUpper();
            }

            // Validar código comercial si se está actualizando
            if (request.CodigoComer != null && request.CodigoComer.ToUpper() != product.CodigoComer.ToUpper())
            {
                var codigoComercialExists = await _productRepository.IsCodigoComercialExistsAsync(request.CodigoComer, id);

                if (codigoComercialExists)
                    return BadRequest(new { message = $"El código comercial '{request.CodigoComer}' ya existe" });

                product.CodigoComer = request.CodigoComer.ToUpper();
            }

            if (request.Producto != null)
                product.Producto = request.Producto;

            if (request.Descripcion != null)
                product.Descripcion = request.Descripcion;

            if (request.FichaTecnica != null)
                product.FichaTecnica = request.FichaTecnica;

            // Actualizar imágenes (permitir null para eliminarlas)
            product.ImagenPrincipal = request.ImagenPrincipal;
            product.Imagen2 = request.Imagen2;
            product.Imagen3 = request.Imagen3;
            product.Imagen4 = request.Imagen4;

            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                    return BadRequest(new { message = "La categoría especificada no existe" });
                product.CategoryId = request.CategoryId.Value;
            }

            if (request.MarcaId.HasValue)
            {
                var marca = await _nombreMarcaRepository.GetByIdAsync(request.MarcaId.Value);
                if (marca == null)
                    return BadRequest(new { message = "La marca especificada no existe" });
                product.MarcaId = request.MarcaId.Value;
            }

            if (request.IsActive.HasValue)
                product.IsActive = request.IsActive.Value;

            if (request.IsFeatured.HasValue)
                product.IsFeatured = request.IsFeatured.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Producto {Codigo} actualizado por {CurrentUser}", product.Codigo, User.Identity?.Name);

            // Invalidar caches
            _cacheService.RemoveByPrefix(CacheKeys.ProductsPrefix);

            var updatedProduct = await _productRepository.GetByIdWithCategoryAsync(id);
            var response = new ProductResponseDto
            {
                Id = updatedProduct!.Id,
                Codigo = updatedProduct.Codigo,
                CodigoComer = updatedProduct.CodigoComer,
                Producto = updatedProduct.Producto,
                Descripcion = updatedProduct.Descripcion,
                FichaTecnica = updatedProduct.FichaTecnica,
                ImagenPrincipal = updatedProduct.ImagenPrincipal,
                Imagen2 = updatedProduct.Imagen2,
                Imagen3 = updatedProduct.Imagen3,
                Imagen4 = updatedProduct.Imagen4,
                IsActive = updatedProduct.IsActive,
                IsFeatured = updatedProduct.IsFeatured,
                CreatedAt = updatedProduct.CreatedAt,
                UpdatedAt = updatedProduct.UpdatedAt,
                CategoryId = updatedProduct.CategoryId,
                CategoryName = updatedProduct.Category.Name,
                MarcaId = updatedProduct.MarcaId,
                MarcaNombre = updatedProduct.Marca.Nombre
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al actualizar el producto", ex);
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateProductStatusRequestDto request)
    {
        try
        {
            await _productRepository.UpdateStatusAsync(id, request.IsActive);
            
            _logger.LogInformation("Estado de producto {Id} actualizado a {Status} por {CurrentUser}", 
                id, request.IsActive, User.Identity?.Name);

            // Invalidar caches
            _cacheService.RemoveByPrefix(CacheKeys.ProductsPrefix);
            _cacheService.Remove(CacheKeys.PublicBrands);
            _cacheService.Remove(CacheKeys.PublicCategories);

            return Ok(new { message = "Estado actualizado correctamente" });
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al actualizar estado del producto", ex);
        }
    }

    [HttpPatch("{id}/featured")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> UpdateFeatured(int id, [FromBody] UpdateProductFeaturedRequestDto request)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            // ✅ VALIDACIÓN: Límite máximo de 9 productos destacados
            const int MAX_FEATURED_PRODUCTS = 9;
            if (request.IsFeatured && !product.IsFeatured)
            {
                // Solo validar si se está intentando activar (de false a true)
                var currentFeaturedCount = await _productRepository.CountFeaturedActiveProductsAsync();
                if (currentFeaturedCount >= MAX_FEATURED_PRODUCTS)
                {
                    return BadRequest(new { 
                        message = $"Solo se permiten {MAX_FEATURED_PRODUCTS} productos destacados. Desactive uno existente antes de activar otro.",
                        currentCount = currentFeaturedCount,
                        maxAllowed = MAX_FEATURED_PRODUCTS
                    });
                }
            }

            await _productRepository.UpdateFeaturedAsync(id, request.IsFeatured);

            _logger.LogInformation("Destacado de producto {Id} actualizado a {Status} por {CurrentUser}",
                id, request.IsFeatured, User.Identity?.Name);

            // Invalidar caches
            _cacheService.RemoveByPrefix(CacheKeys.ProductsPrefix);

            return Ok(new { message = "Destacado actualizado correctamente", isFeatured = request.IsFeatured });
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al actualizar destacado del producto", ex);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        await _productRepository.DeleteAsync(product);
        _logger.LogInformation("Producto {Codigo} eliminado por {CurrentUser}", product.Codigo, User.Identity?.Name);

        // Invalidar caches
        _cacheService.RemoveByPrefix(CacheKeys.ProductsPrefix);

        return Ok(new { message = "Producto eliminado correctamente" });
    }

    /// <summary>
    /// Obtiene productos disponibles públicos (solo activos con imágenes) sin autenticación
    /// </summary>
    [HttpGet("public/active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicActive(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 16,
        [FromQuery] string? q = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? brandIds = null)
    {
        try
        {
            // ✅ SEGURIDAD: Parse brand IDs con validación
            int[]? brandIdsArray = null;
            if (!string.IsNullOrEmpty(brandIds))
            {
                try
                {
                    brandIdsArray = brandIds.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s =>
                        {
                            if (!int.TryParse(s.Trim(), out var id) || id <= 0)
                                throw new FormatException($"ID de marca inválido: {s}");
                            return id;
                        })
                        .ToArray();
                }
                catch (FormatException ex)
                {
                    return BadRequest(new { message = "IDs de marca inválidos", error = ex.Message });
                }
            }

            var (items, total) = await _productRepository.GetPublicActiveProductsPagedAsync(
                page, pageSize, q, categoryId, brandIdsArray);

            var response = new PaginatedProductsResponseDto
            {
                Items = items.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    CodigoComer = p.CodigoComer,
                    Producto = p.Producto,
                    Descripcion = p.Descripcion,
                    FichaTecnica = p.FichaTecnica,
                    ImagenPrincipal = p.ImagenPrincipal,
                    Imagen2 = p.Imagen2,
                    Imagen3 = p.Imagen3,
                    Imagen4 = p.Imagen4,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    MarcaId = p.MarcaId,
                    MarcaNombre = p.Marca.Nombre
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener productos disponibles públicos", ex);
        }
    }

    /// <summary>
    /// Obtiene productos destacados públicos (solo activos con imágenes) sin autenticación
    /// </summary>
    [HttpGet("public/featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicFeatured(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9)
    {
        try
        {
            var (items, total) = await _productRepository.GetPublicFeaturedProductsPagedAsync(page, pageSize);

            var response = new PaginatedProductsResponseDto
            {
                Items = items.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    CodigoComer = p.CodigoComer,
                    Producto = p.Producto,
                    Descripcion = p.Descripcion,
                    FichaTecnica = p.FichaTecnica,
                    ImagenPrincipal = p.ImagenPrincipal,
                    Imagen2 = p.Imagen2,
                    Imagen3 = p.Imagen3,
                    Imagen4 = p.Imagen4,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    MarcaId = p.MarcaId,
                    MarcaNombre = p.Marca.Nombre
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener productos destacados públicos", ex);
        }
    }

    [HttpPost("bulk-import")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> BulkImport([FromBody] BulkImportProductRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.Products == null || request.Products.Count == 0)
            return BadRequest(new { message = "No hay productos para importar" });

        // ✅ SEGURIDAD: Limitar cantidad máxima de productos por importación
        const int MAX_BULK_IMPORT = 1000;
        if (request.Products.Count > MAX_BULK_IMPORT)
            return BadRequest(new { message = $"Máximo {MAX_BULK_IMPORT} productos por importación. Recibido: {request.Products.Count}" });

        var result = new BulkImportResultDto();
        
        // Cache de códigos existentes
        var existingCodes = (await _productRepository.GetAllAsync())
            .Select(p => p.Codigo.ToLower())
            .ToHashSet();

        // Cache de marcas y categorías existentes
        var allMarcas = (await _nombreMarcaRepository.GetAllAsync()).ToList();
        var allCategories = (await _categoryRepository.GetAllAsync()).ToList();
        
        var marcasCache = allMarcas.ToDictionary(m => m.Nombre.ToLower().Trim(), m => m.Id);
        var categoriasCache = allCategories.ToDictionary(c => c.Name.ToLower().Trim(), c => c.Id);

        _logger.LogInformation("Iniciando importación de {Count} productos", request.Products.Count);
        
        // Lista para acumular productos válidos para bulk insert
        var productsToInsert = new List<Product>();

        foreach (var item in request.Products)
        {
            try
            {
                // Verificar duplicados por código
                if (existingCodes.Contains(item.Codigo.ToLower()))
                {
                    result.Duplicates++;
                    continue;
                }

                // Resolver marca (por ID o por nombre)
                int marcaId = 0;
                if (item.MarcaId.HasValue && item.MarcaId.Value > 0)
                {
                    marcaId = item.MarcaId.Value;
                }
                else if (!string.IsNullOrWhiteSpace(item.MarcaNombre))
                {
                    var marcaNombreLower = item.MarcaNombre.ToLower().Trim();
                    
                    if (marcasCache.TryGetValue(marcaNombreLower, out var existingMarcaId))
                    {
                        marcaId = existingMarcaId;
                    }
                    else if (request.AutoCreateEntities)
                    {
                        _logger.LogInformation("Creando marca: {MarcaNombre}", item.MarcaNombre);
                        var newMarca = new NombreMarca
                        {
                            Nombre = item.MarcaNombre.Trim(),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        var createdMarca = await _nombreMarcaRepository.CreateAsync(newMarca);
                        marcaId = createdMarca.Id;
                        marcasCache[marcaNombreLower] = marcaId;
                        result.MarcasCreated++;
                        _logger.LogInformation("Marca creada con ID: {MarcaId}", marcaId);
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add($"Marca '{item.MarcaNombre}' no existe para producto '{item.Codigo}'");
                        continue;
                    }
                }

                if (marcaId == 0)
                {
                    result.Failed++;
                    result.Errors.Add($"Marca no especificada para producto '{item.Codigo}'");
                    continue;
                }

                // Resolver categoría (por ID o por nombre)
                int categoryId = 0;
                if (item.CategoryId.HasValue && item.CategoryId.Value > 0)
                {
                    categoryId = item.CategoryId.Value;
                }
                else if (!string.IsNullOrWhiteSpace(item.CategoriaNombre))
                {
                    var categoriaNombreLower = item.CategoriaNombre.ToLower().Trim();
                    
                    if (categoriasCache.TryGetValue(categoriaNombreLower, out var existingCategoryId))
                    {
                        categoryId = existingCategoryId;
                    }
                    else if (request.AutoCreateEntities)
                    {
                        var newCategory = new Category
                        {
                            Name = item.CategoriaNombre.Trim(),
                            Description = $"Categoría importada: {item.CategoriaNombre.Trim()}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _categoryRepository.AddAsync(newCategory);
                        categoryId = newCategory.Id;
                        categoriasCache[categoriaNombreLower] = categoryId;
                        result.CategoriasCreated++;
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add($"Categoría '{item.CategoriaNombre}' no existe para producto '{item.Codigo}'");
                        continue;
                    }
                }

                if (categoryId == 0)
                {
                    result.Failed++;
                    result.Errors.Add($"Categoría no especificada para producto '{item.Codigo}'");
                    continue;
                }

                // Usar Codigo como CodigoComer si está vacío
                var codigoComer = string.IsNullOrWhiteSpace(item.CodigoComer) ? item.Codigo : item.CodigoComer;

            var product = new Product
            {
                Codigo = item.Codigo,
                CodigoComer = codigoComer,
                Producto = item.Producto,
                CategoryId = categoryId,
                MarcaId = marcaId,
                Descripcion = item.Descripcion,
                FichaTecnica = item.FichaTecnica,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsFeatured = false
            };

                // Agregar a la lista para bulk insert
                productsToInsert.Add(product);
                existingCodes.Add(item.Codigo.ToLower());
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Error al procesar '{item.Codigo}': {ex.Message}");
                _logger.LogError(ex, "Error al procesar producto {Codigo}", item.Codigo);
            }
        }

        // Bulk insert de todos los productos válidos
        if (productsToInsert.Count > 0)
        {
            try
            {
                _logger.LogInformation("Ejecutando bulk insert de {Count} productos", productsToInsert.Count);
                await _productRepository.BulkInsertAsync(productsToInsert);
                result.Imported = productsToInsert.Count;
                _logger.LogInformation("Bulk insert completado exitosamente");
            }
            catch (Exception ex)
            {
                result.Failed += productsToInsert.Count;
                result.Imported = 0;
                result.Errors.Add($"Error en bulk insert: {ex.Message}");
                _logger.LogError(ex, "Error al ejecutar bulk insert");
            }
        }

        _logger.LogInformation(
            "Importación masiva completada por {CurrentUser}: {Imported} importados, {Failed} fallidos, {Duplicates} duplicados, {MarcasCreated} marcas creadas, {CategoriasCreated} categorías creadas",
            User.Identity?.Name, result.Imported, result.Failed, result.Duplicates, result.MarcasCreated, result.CategoriasCreated);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene todas las marcas activas públicamente (sin autenticación)
    /// Solo devuelve marcas que tienen productos activos con imágenes
    /// </summary>
    [HttpGet("public/brands")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicBrands()
    {
        try
        {
            var brands = await _cacheService.GetOrCreateAsync(CacheKeys.PublicBrands, async () =>
            {
                var brandIds = await _productRepository.GetDistinctBrandIdsWithActiveProductsAsync();

                // ✅ OPTIMIZADO: Filtra directamente en BD en lugar de cargar todas y filtrar en memoria
                var marcas = await _nombreMarcaRepository.GetActiveByIdsAsync(brandIds);

                return marcas
                    .Select(m => new { Id = m.Id, Nombre = m.Nombre })
                    .ToList();
            }, CacheExpiration.PublicMetadata);

            return Ok(brands);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener marcas públicas", ex);
        }
    }

    /// <summary>
    /// Obtiene todas las categorías activas públicamente con conteo de productos activos (sin autenticación)
    /// Solo devuelve categorías que tienen productos activos con imágenes
    /// </summary>
    [HttpGet("public/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicCategories()
    {
        try
        {
            var activeCategories = await _cacheService.GetOrCreateAsync(CacheKeys.PublicCategories, async () =>
            {
                var categoryIds = await _productRepository.GetDistinctCategoryIdsWithActiveProductsAsync();

                // ✅ OPTIMIZADO: Filtra directamente en BD en lugar de cargar todas y filtrar en memoria
                var categories = await _categoryRepository.GetActiveByIdsAsync(categoryIds);

                return categories
                    .Select(c => new
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToList();
            }, CacheExpiration.PublicMetadata);

            return Ok(activeCategories);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener categorías públicas", ex);
        }
    }
}
