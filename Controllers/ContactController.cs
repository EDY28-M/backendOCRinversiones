using Microsoft.AspNetCore.Mvc;
using backendORCinverisones.Application.DTOs.Contact;
using backendORCinverisones.Application.Interfaces.Services;

namespace backendORCinverisones.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendContact([FromBody] ContactRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (adminSent, userSent) = await _contactService.SendContactAsync(request, cancellationToken);

        if (!adminSent)
        {
            _logger.LogWarning("No se pudo enviar el email de contacto para {Email}", request.Email);
            return StatusCode(500, new { message = "No pudimos enviar tu mensaje en este momento. Inténtalo más tarde." });
        }

        if (!userSent)
        {
            return Ok(new
            {
                message = "Mensaje recibido. No pudimos enviar el email de confirmación.",
                confirmationSent = false
            });
        }

        return Ok(new
        {
            message = "Mensaje enviado correctamente. Revisa tu correo para la confirmación.",
            confirmationSent = true
        });
    }
}
