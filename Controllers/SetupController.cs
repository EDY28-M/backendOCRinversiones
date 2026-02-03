using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendORCinverisones.Infrastructure.Data;

namespace backendORCinverisones.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SetupController> _logger;

        public SetupController(ApplicationDbContext context, ILogger<SetupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint para Render
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            var health = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                database = "unknown"
            };

            try
            {
                // Probar conexión a la base de datos
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new
                {
                    status = canConnect ? "healthy" : "unhealthy",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    database = canConnect ? "connected" : "disconnected"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed - Database connection error");
                return Ok(new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    database = "error",
                    error = ex.Message
                });
            }
        }

        [HttpGet("fix-admin")]
        public async Task<IActionResult> FixAdminPassword()
        {
            // Hash para 'Admin123!' generado con BCrypt.Net
            var validHash = "$2a$11$coMcJF6JoQYOAbzXPavCveSDeNIZ8yeIvhuhUBAIngZyUKdD9cFCa";

            var sql = @"
                UPDATE [Users]
                SET [PasswordHash] = {0},
                    [RoleId] = 1,
                    [IsActive] = 1
                WHERE [Username] = 'admin'";

            await _context.Database.ExecuteSqlRawAsync(sql, validHash);

            return Ok(new { message = "Contraseña de admin reseteada a: Admin123!" });
        }
    }
}
