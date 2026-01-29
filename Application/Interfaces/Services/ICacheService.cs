namespace backendORCinverisones.Application.Interfaces.Services;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}
