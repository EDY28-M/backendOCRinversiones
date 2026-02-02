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

        public SetupController(ApplicationDbContext context)
        {
            _context = context;
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
