using Microsoft.EntityFrameworkCore;
using backendORCinverisones.Domain.Entities;

namespace backendORCinverisones.Infrastructure.Data;

/// <summary>
/// ✅ QUERIES COMPILADAS DE EF CORE
/// 
/// Las consultas compiladas (Compiled Queries) son pre-compiladas por EF Core,
/// eliminando el overhead de parsing y compilation en cada ejecución.
/// 
/// Beneficios:
/// - Hasta 10x más rápidas en la primera ejecución
/// - Menor uso de memoria (sin re-compilación)
/// - Mejor rendimiento en escenarios de alta carga
/// 
/// https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics#compiled-queries
/// </summary>
public static class CompiledQueries
{
    // ============================================
    // PRODUCT QUERIES
    // ============================================
    
    /// <summary>
    /// Obtiene un producto por ID con sus relaciones
    /// </summary>
    private static readonly Func<ApplicationDbContext, int, Task<Product?>> GetProductByIdCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, int id) =>
                context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Marca)
                    .FirstOrDefault(p => p.Id == id));

    public static Task<Product?> GetProductByIdAsync(ApplicationDbContext context, int id)
        => GetProductByIdCompiled(context, id);

    /// <summary>
    /// Verifica si un código existe
    /// </summary>
    private static readonly Func<ApplicationDbContext, string, Task<bool>> CheckCodigoExistsCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string codigo) =>
                context.Products
                    .AsNoTracking()
                    .Any(p => p.Codigo.ToUpper() == codigo.ToUpper()));

    public static Task<bool> CheckCodigoExistsAsync(ApplicationDbContext context, string codigo)
        => CheckCodigoExistsCompiled(context, codigo);

    /// <summary>
    /// Obtiene IDs de productos activos
    /// </summary>
    private static readonly Func<ApplicationDbContext, IAsyncEnumerable<int>> GetActiveProductIdsCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context) =>
                context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .Select(p => p.Id));

    public static IAsyncEnumerable<int> GetActiveProductIdsStream(ApplicationDbContext context)
        => GetActiveProductIdsCompiled(context);

    // ============================================
    // USER QUERIES
    // ============================================
    
    /// <summary>
    /// Obtiene usuario por username con su rol
    /// </summary>
    private static readonly Func<ApplicationDbContext, string, Task<User?>> GetUserByUsernameCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string username) =>
                context.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Username == username));

    public static Task<User?> GetUserByUsernameAsync(ApplicationDbContext context, string username)
        => GetUserByUsernameCompiled(context, username);

    /// <summary>
    /// Obtiene usuario por email
    /// </summary>
    private static readonly Func<ApplicationDbContext, string, Task<User?>> GetUserByEmailCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string email) =>
                context.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Email == email));

    public static Task<User?> GetUserByEmailAsync(ApplicationDbContext context, string email)
        => GetUserByEmailCompiled(context, email);

    /// <summary>
    /// Verifica si existe un usuario por username
    /// </summary>
    private static readonly Func<ApplicationDbContext, string, Task<bool>> CheckUsernameExistsCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, string username) =>
                context.Users
                    .AsNoTracking()
                    .Any(u => u.Username == username));

    public static Task<bool> CheckUsernameExistsAsync(ApplicationDbContext context, string username)
        => CheckUsernameExistsCompiled(context, username);

    // ============================================
    // CATEGORY QUERIES
    // ============================================
    
    /// <summary>
    /// Obtiene conteo de productos por categoría
    /// </summary>
    private static readonly Func<ApplicationDbContext, int, Task<int>> CountProductsByCategoryCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, int categoryId) =>
                context.Products
                    .AsNoTracking()
                    .Count(p => p.CategoryId == categoryId && p.IsActive));

    public static Task<int> CountProductsByCategoryAsync(ApplicationDbContext context, int categoryId)
        => CountProductsByCategoryCompiled(context, categoryId);

    /// <summary>
    /// Obtiene todas las categorías activas (como Task en lugar de IAsyncEnumerable)
    /// </summary>
    private static readonly Func<ApplicationDbContext, Task<List<Category>>> GetActiveCategoriesCompiled =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context) =>
                context.Categories
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToList());

    public static Task<List<Category>> GetActiveCategoriesAsync(ApplicationDbContext context)
        => GetActiveCategoriesCompiled(context);
}
