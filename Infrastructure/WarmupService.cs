using Microsoft.EntityFrameworkCore;
using backendORCinverisones.Infrastructure.Data;
using Serilog;

namespace backendORCinverisones.Infrastructure;

/// <summary>
/// Servicio de warmup que pre-calienta la aplicación al iniciar.
/// Reduce drásticamente el tiempo de respuesta del primer request (cold start).
/// </summary>
public class WarmupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public WarmupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("🔥 Iniciando warmup de la aplicación...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Pre-calentar conexión a DB y pool de conexiones
            Log.Information("🔥 [Warmup] Calentando conexión a base de datos...");
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            Log.Information("🔥 [Warmup] Conexión DB: {Status}", canConnect ? "OK" : "FALLO");

            if (canConnect)
            {
                // 2. Pre-compilar query de login (EF Core compila la query en el primer uso)
                Log.Information("🔥 [Warmup] Pre-compilando queries de EF Core...");
                _ = await dbContext.Users
                    .AsNoTracking()
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == -1, cancellationToken); // Query que nunca retorna datos pero compila el modelo

                // 3. Pre-calentar tabla de roles (lookup table pequeña, se usa en cada login)
                _ = await dbContext.Roles
                    .AsNoTracking()
                    .CountAsync(cancellationToken);

                Log.Information("🔥 [Warmup] Queries pre-compiladas correctamente");
            }

            // 4. Pre-calentar BCrypt (la primera llamada a BCrypt es más lenta)
            Log.Information("🔥 [Warmup] Pre-calentando BCrypt...");
            _ = BCrypt.Net.BCrypt.HashPassword("warmup", workFactor: 10);
            Log.Information("🔥 [Warmup] BCrypt listo");

            sw.Stop();
            Log.Information("🔥 Warmup completado en {Elapsed}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            Log.Warning(ex, "⚠️ Warmup parcialmente fallido en {Elapsed}ms (la app sigue funcionando)", sw.ElapsedMilliseconds);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
