namespace backendORCinverisones.Application.Interfaces.Services;

/// <summary>
/// Interfaz para servicio de caché
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene o crea un valor en caché
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave de caché</param>
    /// <param name="factory">Función para generar el valor si no existe</param>
    /// <param name="absoluteExpiration">Tiempo de expiración opcional</param>
    /// <returns>El valor en caché</returns>
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null);

    /// <summary>
    /// Elimina una clave específica de la caché
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Elimina todas las claves que comienzan con el prefijo especificado
    /// </summary>
    void RemoveByPrefix(string prefix);
}
