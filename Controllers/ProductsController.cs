using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Products;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Application.Logging;
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
    private readonly IImageCompressionService _imageCompressionService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        INombreMarcaRepository nombreMarcaRepository,
        ICodeGeneratorService codeGeneratorService,
        ICacheService cacheService,
        IImageCompressionService imageCompressionService,
        IWebHostEnvironment env,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _nombreMarcaRepository = nombreMarcaRepository;
        _codeGeneratorService = codeGeneratorService;
        _cacheService = cacheService;
        _imageCompressionService = imageCompressionService;
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

    /// <summary>
    /// Sanitiza URL de imagen: retorna null si no es una URL válida (http/https o data:image)
    /// Previene que URLs corruptas/inválidas lleguen al frontend público
    /// </summary>
    private static string? SanitizeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var trimmed = url.Trim();
        if (trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)) return trimmed;
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
            (uri.Scheme == "http" || uri.Scheme == "https"))
        {
            return trimmed;
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _productRepository.GetAllForListAsync();
        return Ok(response);
    }

    /// <summary>
    /// Obtiene los últimos productos creados (historial reciente).
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 10)
    {
        try
        {
            // Validar rango: mínimo 1, máximo 100
            limit = Math.Clamp(limit, 1, 100);

            var recentProducts = await _productRepository.GetRecentProductsAsync(limit);
            return Ok(recentProducts);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener productos recientes", ex);
        }
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

    [HttpGet("{id:int}")]
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

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            var marca = await _nombreMarcaRepository.GetByIdAsync(request.MarcaId);

            if (category == null)
                return BadRequest(new { message = "La categoría especificada no existe" });
            if (marca == null)
                return BadRequest(new { message = "La marca especificada no existe" });

            // ✅ IMÁGENES: Ahora se almacenan como URLs (Backblaze, etc.)
            // Ya no se hace compresión Base64, se guardan las URLs directamente
            var product = new Product
            {
                Codigo = request.Codigo.ToUpper(),
                CodigoComer = request.CodigoComer.ToUpper(),
                Producto = request.Producto,
                Descripcion = request.Descripcion,
                FichaTecnica = request.FichaTecnica,
                // URLs de imágenes (Backblaze u otro CDN)
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
            
            // ✅ NUEVA ARQUITECTURA: También guardamos en la nueva tabla ProductImages
            // Solo para las imágenes que existan (no nulas/vacías)
            /* NOTA: Activaremos esto en la siguiente fase de refactorización completa
            if (!string.IsNullOrEmpty(product.ImagenPrincipal)) product.Images.Add(new ProductImage { ImageUrl = product.ImagenPrincipal, OrderIndex = 0 });
            if (!string.IsNullOrEmpty(product.Imagen2)) product.Images.Add(new ProductImage { ImageUrl = product.Imagen2, OrderIndex = 1 });
            if (!string.IsNullOrEmpty(product.Imagen3)) product.Images.Add(new ProductImage { ImageUrl = product.Imagen3, OrderIndex = 2 });
            if (!string.IsNullOrEmpty(product.Imagen4)) product.Images.Add(new ProductImage { ImageUrl = product.Imagen4, OrderIndex = 3 });
            */

            await _productRepository.AddAsync(product);
            _logger.LogInformation("Producto {Codigo} creado por {CurrentUser}", product.Codigo, User.Identity?.Name);

            _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);

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

    [HttpPut("{id:int}")]
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

            // ✅ VALIDACIONES SECUENCIALES (evita DbContext concurrency issues)
            // EF Core no soporta operaciones paralelas en el mismo DbContext
            
            // Validar código si se está actualizando
            if (request.Codigo != null && request.Codigo.ToUpper() != product.Codigo.ToUpper())
            {
                if (await _productRepository.IsCodigoExistsAsync(request.Codigo, id))
                    return BadRequest(new { message = $"El código '{request.Codigo}' ya existe" });
            }

            // Validar código comercial si se está actualizando
            if (request.CodigoComer != null && request.CodigoComer.ToUpper() != product.CodigoComer.ToUpper())
            {
                if (await _productRepository.IsCodigoComercialExistsAsync(request.CodigoComer, id))
                    return BadRequest(new { message = $"El código comercial '{request.CodigoComer}' ya existe" });
            }
            
            // Validar existencia de Categoría si cambió
            if (request.CategoryId.HasValue && request.CategoryId != product.CategoryId)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null) 
                    return BadRequest(new { message = "La categoría especificada no existe" });
                product.CategoryId = request.CategoryId.Value;
            }

            // Validar existencia de Marca si cambió
            if (request.MarcaId.HasValue && request.MarcaId != product.MarcaId)
            {
                var marca = await _nombreMarcaRepository.GetByIdAsync(request.MarcaId.Value);
                if (marca == null) 
                    return BadRequest(new { message = "La marca especificada no existe" });
                product.MarcaId = request.MarcaId.Value;
            }

            // ACTUALIZAR PROPIEDADES DIRECTAS
            if (request.Codigo != null) product.Codigo = request.Codigo.ToUpper();
            if (request.CodigoComer != null) product.CodigoComer = request.CodigoComer.ToUpper();
            if (request.Producto != null) product.Producto = request.Producto;
            if (request.Descripcion != null) product.Descripcion = request.Descripcion;
            if (request.FichaTecnica != null) product.FichaTecnica = request.FichaTecnica;
            if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
            if (request.IsFeatured.HasValue) product.IsFeatured = request.IsFeatured.Value;

            // ✅ IMÁGENES: Ahora se almacenan como URLs directamente (Backblaze, etc.)
            // Si se envía string vacío, se limpia la imagen (permite quitar imágenes)
            if (request.ImagenPrincipal != null) product.ImagenPrincipal = string.IsNullOrWhiteSpace(request.ImagenPrincipal) ? null : request.ImagenPrincipal;
            if (request.Imagen2 != null) product.Imagen2 = string.IsNullOrWhiteSpace(request.Imagen2) ? null : request.Imagen2;
            if (request.Imagen3 != null) product.Imagen3 = string.IsNullOrWhiteSpace(request.Imagen3) ? null : request.Imagen3;
            if (request.Imagen4 != null) product.Imagen4 = string.IsNullOrWhiteSpace(request.Imagen4) ? null : request.Imagen4;

            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            _logger.ProductUpdated(product.Codigo, User.Identity?.Name);

            // Invalidar caches
            _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);

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

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateProductStatusRequestDto request)
    {
        try
        {
            await _productRepository.UpdateStatusAsync(id, request.IsActive);
            
            _logger.LogInformation("Estado de producto {Id} actualizado a {Status} por {CurrentUser}", 
                id, request.IsActive, User.Identity?.Name);

            // Invalidar caches
            _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);
            _cacheService.Remove(Application.Constants.CacheKeys.PublicBrands);
            _cacheService.Remove(Application.Constants.CacheKeys.PublicCategories);

            return Ok(new { message = "Estado actualizado correctamente" });
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al actualizar estado del producto", ex);
        }
    }

    [HttpPatch("{id:int}/featured")]
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
            _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);

            return Ok(new { message = "Destacado actualizado correctamente", isFeatured = request.IsFeatured });
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al actualizar destacado del producto", ex);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        await _productRepository.DeleteAsync(product);
        _logger.ProductDeleted(product.Codigo, User.Identity?.Name);

        // Invalidar caches
        _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);

        return Ok(new { message = "Producto eliminado correctamente" });
    }

    [HttpDelete("delete-all")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            await _productRepository.DeleteAllAsync();
            _logger.LogInformation("TODOS los productos han sido eliminados por {CurrentUser}", User.Identity?.Name);
            
            // Invalidar caches
            _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);
            _cacheService.Remove(Application.Constants.CacheKeys.PublicBrands);
            _cacheService.Remove(Application.Constants.CacheKeys.PublicCategories);

            return Ok(new { message = "Todos los productos han sido eliminados correctamente" });
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al eliminar todos los productos", ex);
        }
    }

    /// <summary>
    /// Obtiene productos disponibles públicos (solo activos con imágenes) sin autenticación
    /// </summary>
    [HttpGet("public/active")]
    [AllowAnonymous]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "page", "pageSize", "q", "categoryId", "brandIds" })]
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
                    ImagenPrincipal = SanitizeImageUrl(p.ImagenPrincipal),
                    Imagen2 = SanitizeImageUrl(p.Imagen2),
                    Imagen3 = SanitizeImageUrl(p.Imagen3),
                    Imagen4 = SanitizeImageUrl(p.Imagen4),
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
    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "page", "pageSize" })]
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
                    ImagenPrincipal = SanitizeImageUrl(p.ImagenPrincipal),
                    Imagen2 = SanitizeImageUrl(p.Imagen2),
                    Imagen3 = SanitizeImageUrl(p.Imagen3),
                    Imagen4 = SanitizeImageUrl(p.Imagen4),
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

        try
        {

        // ✅ SEGURIDAD: Validar límite (DESHABILITADO POR SOLICITUD DE USUARIO)
        // const int MAX_BULK_IMPORT = 1000;
        // if (request.Products.Count > MAX_BULK_IMPORT)
        //     return BadRequest(new { message = $"Máximo {MAX_BULK_IMPORT} productos por importación. Recibido: {request.Products.Count}" });

        var result = new BulkImportResultDto();
        
        // Cache de códigos existentes (Optimizado: Solo traer códigos, no entidades completas)
        var existingCodesInfo = await _productRepository.GetCodigosForGenerationAsync();
        var existingCodes = existingCodesInfo.Select(x => x.Codigo.ToLower()).ToHashSet();

        _logger.LogInformation("Iniciando importación OPTIMIZADA de {Count} productos", request.Products.Count);

        // 1. Identificar Marcas y Categorías nuevas vs existentes
        // Traemos todas para asegurar mapeo correcto (optimización: solo id y nombre si fuera posible, pero repository trae todo, ok)
        var allMarcas = await _nombreMarcaRepository.GetAllAsync();
        var allCategories = await _categoryRepository.GetAllAsync();

        var marcasCache = allMarcas.ToDictionary(m => m.Nombre.ToLower().Trim(), m => m.Id);
        var categoriasCache = allCategories.ToDictionary(c => c.Name.ToLower().Trim(), c => c.Id);

        var marcasToCreate = new Dictionary<string, NombreMarca>();
        var categoriasToCreate = new Dictionary<string, Category>();
        var productsToInsert = new List<Product>();

        // 2. Primera pasada: Identificar dependencias faltantes
        foreach (var item in request.Products)
        {
            if (existingCodes.Contains(item.Codigo.ToLower()))
            {
                result.Duplicates++;
                continue;
            }

            // Marca
            if (!item.MarcaId.HasValue && !string.IsNullOrWhiteSpace(item.MarcaNombre))
            {
                var marcaKey = item.MarcaNombre.ToLower().Trim();
                if (!marcasCache.ContainsKey(marcaKey) && !marcasToCreate.ContainsKey(marcaKey))
                {
                    if (request.AutoCreateEntities)
                    {
                        marcasToCreate[marcaKey] = new NombreMarca
                        {
                            Nombre = item.MarcaNombre.Trim(),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                }
            }

            // Categoría
            if (!item.CategoryId.HasValue && !string.IsNullOrWhiteSpace(item.CategoriaNombre))
            {
                var catKey = item.CategoriaNombre.ToLower().Trim();
                if (!categoriasCache.ContainsKey(catKey) && !categoriasToCreate.ContainsKey(catKey))
                {
                    if (request.AutoCreateEntities)
                    {
                        categoriasToCreate[catKey] = new Category
                        {
                            Name = item.CategoriaNombre.Trim(),
                            Description = $"Categoría importada: {item.CategoriaNombre.Trim()}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                    }
                }
            }
        }

        // 3. Crear dependencias masivamente
        if (marcasToCreate.Count > 0)
        {
            _logger.LogInformation("Creando {Count} nuevas marcas masivamente...", marcasToCreate.Count);
            await _nombreMarcaRepository.CreateRangeAsync(marcasToCreate.Values);
            result.MarcasCreated = marcasToCreate.Count;
            
            // Reconstruir cache o agregar las nuevas con sus IDs generados
            foreach (var m in marcasToCreate.Values)
            {
                marcasCache[m.Nombre.ToLower().Trim()] = m.Id;
            }
        }

        if (categoriasToCreate.Count > 0)
        {
            _logger.LogInformation("Creando {Count} nuevas categorías masivamente...", categoriasToCreate.Count);
            // CategoryRepository hereda de Repository<T>, debe tener AddRangeAsync
            await _categoryRepository.AddRangeAsync(categoriasToCreate.Values);
            result.CategoriasCreated = categoriasToCreate.Count;
            
            // Reconstruir cache
            foreach (var c in categoriasToCreate.Values)
            {
                categoriasCache[c.Name.ToLower().Trim()] = c.Id;
            }
        }

        // 4. Segunda pasada: Construir productos con IDs resueltos
        foreach (var item in request.Products)
        {
            // Skip duplicados (ya contados arriba)
            if (existingCodes.Contains(item.Codigo.ToLower())) continue;

            int marcaId = 0;
            if (item.MarcaId.HasValue && item.MarcaId.Value > 0)
            {
                marcaId = item.MarcaId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(item.MarcaNombre))
            {
                if (marcasCache.TryGetValue(item.MarcaNombre.ToLower().Trim(), out var id))
                {
                    marcaId = id;
                }
            }

            int categoryId = 0;
            if (item.CategoryId.HasValue && item.CategoryId.Value > 0)
            {
                categoryId = item.CategoryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(item.CategoriaNombre))
            {
                if (categoriasCache.TryGetValue(item.CategoriaNombre.ToLower().Trim(), out var id))
                {
                    categoryId = id;
                }
            }

            if (marcaId == 0 || categoryId == 0)
            {
                result.Failed++;
                result.Errors.Add($"Falta Marca o Categoría para '{item.Codigo}'. MarcaID: {marcaId}, CatID: {categoryId}");
                continue;
            }

            // Usar Codigo como CodigoComer si está vacío
            var codigoComer = string.IsNullOrWhiteSpace(item.CodigoComer) ? item.Codigo : item.CodigoComer;

            productsToInsert.Add(new Product
            {
                Codigo = item.Codigo.ToUpper(),
                CodigoComer = codigoComer.ToUpper(),
                Producto = item.Producto,
                CategoryId = categoryId,
                MarcaId = marcaId,
                Descripcion = item.Descripcion,
                FichaTecnica = item.FichaTecnica,
                ImagenPrincipal = item.ImagenPrincipal,
                Imagen2 = item.Imagen2,
                Imagen3 = item.Imagen3,
                Imagen4 = item.Imagen4,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsFeatured = false
            });

            // Evitar procesar el mismo código si viene duplicado en el MISMO request
            existingCodes.Add(item.Codigo.ToLower());
        }

        // 5. Bulk insert de productos
        if (productsToInsert.Count > 0)
        {
            try
            {
                _logger.LogInformation("Ejecutando bulk insert FINAL de {Count} productos", productsToInsert.Count);
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

        // 6. Invalidar caches relacionados (productos, marcas, categorías)
        if (productsToInsert.Count > 0)
        {
            _cacheService.RemoveByPrefix(Application.Constants.CacheKeys.ProductsPrefix);
            _cacheService.Remove(Application.Constants.CacheKeys.PublicBrands);
            _cacheService.Remove(Application.Constants.CacheKeys.PublicCategories);
        }

        if (marcasToCreate.Count > 0)
        {
            _cacheService.RemoveByPrefix("marcas:");
            _cacheService.Remove(Application.Constants.CacheKeys.PublicBrands);
        }

        if (categoriasToCreate.Count > 0)
        {
            _cacheService.RemoveByPrefix("categories:");
            _cacheService.Remove(Application.Constants.CacheKeys.PublicCategories);
        }

        _logger.LogInformation(
            "Importación masiva completada por {CurrentUser}: {Imported} importados, {Failed} fallidos, {Duplicates} duplicados, {MarcasCreated} marcas creadas, {CategoriasCreated} categorías creadas",
            User.Identity?.Name, result.Imported, result.Failed, result.Duplicates, result.MarcasCreated, result.CategoriasCreated);

        return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en BulkImport: {Message} | Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            return StatusCode(500, new 
            { 
                message = $"Error en importación masiva: {ex.Message}",
                innerError = ex.InnerException?.Message,
                errors = new[] { ex.Message, ex.InnerException?.Message }.Where(e => e != null)
            });
        }
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
            var brands = await _cacheService.GetOrCreateAsync(Application.Constants.CacheKeys.PublicBrands, async () =>
            {
                var brandIds = await _productRepository.GetDistinctBrandIdsWithActiveProductsAsync();

                // ✅ OPTIMIZADO: Filtra directamente en BD en lugar de cargar todas y filtrar en memoria
                var marcas = await _nombreMarcaRepository.GetActiveByIdsAsync(brandIds);

                return marcas
                    .Select(m => new { Id = m.Id, Nombre = m.Nombre })
                    .ToList();
            }, Application.Constants.CacheExpiration.PublicMetadata);

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
            var activeCategories = await _cacheService.GetOrCreateAsync(Application.Constants.CacheKeys.PublicCategories, async () =>
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
            }, Application.Constants.CacheExpiration.PublicMetadata);

            return Ok(activeCategories);
        }
        catch (Exception ex)
        {
            return SecureError(500, "Error al obtener categorías públicas", ex);
        }
    }
}
