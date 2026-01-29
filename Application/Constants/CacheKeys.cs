namespace backendORCinverisones.Application.Constants;

public static class CacheKeys
{
    public const string ProductsPrefix = "products:";

    public static string PublicActiveProducts(int page, int pageSize, string? q,
        int? categoryId, string? brandIds)
        => $"{ProductsPrefix}public:p{page}:ps{pageSize}:q{q ?? ""}:c{categoryId}:b{brandIds ?? ""}";

    public static string AvailableProducts(int page, int pageSize, string? q,
        int? categoryId, bool onlyWithImages, bool onlyActive)
        => $"{ProductsPrefix}available:p{page}:ps{pageSize}:q{q ?? ""}:c{categoryId}:img{onlyWithImages}:act{onlyActive}";

    public static string PublicCategories => $"{ProductsPrefix}public:categories";
    public static string PublicBrands => $"{ProductsPrefix}public:brands";
}

public static class CacheExpiration
{
    public static readonly TimeSpan PublicProducts = TimeSpan.FromSeconds(10); // 10 segundos - casi instantáneo
    public static readonly TimeSpan PublicMetadata = TimeSpan.FromSeconds(10); // 10 segundos
    public static readonly TimeSpan AvailableProducts = TimeSpan.FromSeconds(10); // 10 segundos
}
