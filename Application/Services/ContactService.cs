using System.Net;
using backendORCinverisones.Application.DTOs.Contact;
using backendORCinverisones.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backendORCinverisones.Application.Services;

public class ContactService : IContactService
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<ContactService> logger)
    {
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool AdminSent, bool UserSent)> SendContactAsync(
        ContactRequestDto request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var contactRecipient = ResolveContactRecipient();
        if (string.IsNullOrWhiteSpace(contactRecipient))
        {
            _logger.LogError("EmailSettings:ContactRecipientEmail o EmailSettings:SenderEmail no configurado");
            return (false, false);
        }

        var senderName = _configuration["EmailSettings:SenderName"] ?? "ORC Inversiones";
        var templateId = int.Parse(_configuration["EmailSettings:BrevoTemplateId"] ?? "4");

        // Parámetros dinámicos para la plantilla de Brevo
        var templateParams = new Dictionary<string, object>
        {
            ["name"] = request.Name,
            ["email"] = request.Email,
            ["phone"] = string.IsNullOrWhiteSpace(request.Phone) ? "No especificado" : request.Phone,
            ["subject"] = string.IsNullOrWhiteSpace(request.Subject) ? "Consulta desde la web" : request.Subject,
            ["message"] = request.Message,
            ["company"] = "", // El frontend ya incluye empresa en el message
            ["date"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'"),
            ["senderName"] = senderName
        };

        // 1. Email al administrador (notificación de nuevo contacto)
        var adminSent = await _emailService.SendTemplateEmailAsync(
            toEmail: contactRecipient,
            toName: senderName,
            templateId: templateId,
            templateParams: templateParams,
            replyToEmail: request.Email,
            replyToName: request.Name,
            cancellationToken: cancellationToken);

        if (!adminSent)
            return (false, false);

        // 2. Email de confirmación al usuario
        var userParams = new Dictionary<string, object>
        {
            ["name"] = request.Name,
            ["email"] = request.Email,
            ["subject"] = string.IsNullOrWhiteSpace(request.Subject) ? "Consulta desde la web" : request.Subject,
            ["message"] = request.Message,
            ["senderName"] = senderName
        };

        var userSent = await _emailService.SendTemplateEmailAsync(
            toEmail: request.Email,
            toName: request.Name,
            templateId: templateId,
            templateParams: userParams,
            cancellationToken: cancellationToken);

        return (true, userSent);
    }

    private string? ResolveContactRecipient()
    {
        var configured = _configuration["EmailSettings:ContactRecipientEmail"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        var envFallback = Environment.GetEnvironmentVariable("CONTACT_RECIPIENT_EMAIL");
        if (!string.IsNullOrWhiteSpace(envFallback))
            return envFallback;

        return _configuration["EmailSettings:SenderEmail"];
    }
}
