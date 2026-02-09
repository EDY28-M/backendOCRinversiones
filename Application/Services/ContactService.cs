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
        var subjectAdmin = string.IsNullOrWhiteSpace(request.Subject)
            ? "Nuevo contacto desde la web"
            : $"Nuevo contacto: {request.Subject.Trim()}";

        var adminBody = BuildAdminBody(request, senderName);
        var adminSent = await _emailService.SendEmailAsync(
            contactRecipient,
            subjectAdmin,
            adminBody,
            isHtml: true,
            replyToEmail: request.Email,
            replyToName: request.Name,
            cancellationToken: cancellationToken);

        if (!adminSent)
            return (false, false);

        var subjectUser = $"Gracias por contactarnos - {senderName}";
        var userBody = BuildUserBody(request, senderName);

        var userSent = await _emailService.SendEmailAsync(
            request.Email,
            subjectUser,
            userBody,
            isHtml: true,
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

    private static string BuildAdminBody(ContactRequestDto request, string senderName)
    {
        var name = HtmlEncode(request.Name);
        var email = HtmlEncode(request.Email);
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? "No especificado" : HtmlEncode(request.Phone);
        var subject = string.IsNullOrWhiteSpace(request.Subject) ? "(Sin asunto)" : HtmlEncode(request.Subject);
        var message = HtmlEncodeWithLineBreaks(request.Message);
        var sentAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'");

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f7f7f7; padding: 20px;'>
    <div style='max-width: 680px; margin: 0 auto; background: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.08);'>
        <div style='background: #0f2a3d; color: #ffffff; padding: 24px;'>
            <h2 style='margin: 0; font-size: 20px;'>Nuevo contacto desde la web</h2>
            <p style='margin: 8px 0 0 0; font-size: 14px;'>{HtmlEncode(senderName)}</p>
        </div>
        <div style='padding: 24px;'>
            <p style='margin: 0 0 12px 0;'><strong>Nombre:</strong> {name}</p>
            <p style='margin: 0 0 12px 0;'><strong>Email:</strong> {email}</p>
            <p style='margin: 0 0 12px 0;'><strong>Teléfono:</strong> {phone}</p>
            <p style='margin: 0 0 12px 0;'><strong>Asunto:</strong> {subject}</p>
            <p style='margin: 0 0 8px 0;'><strong>Mensaje:</strong></p>
            <div style='background: #f3f4f6; padding: 16px; border-radius: 6px; color: #1f2937; line-height: 1.5;'>
                {message}
            </div>
            <p style='margin: 16px 0 0 0; font-size: 12px; color: #6b7280;'>
                Enviado: {sentAt}
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private static string BuildUserBody(ContactRequestDto request, string senderName)
    {
        var name = HtmlEncode(request.Name);
        var subject = string.IsNullOrWhiteSpace(request.Subject) ? "(Sin asunto)" : HtmlEncode(request.Subject);
        var message = HtmlEncodeWithLineBreaks(request.Message);

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f7f7f7; padding: 20px;'>
    <div style='max-width: 680px; margin: 0 auto; background: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.08);'>
        <div style='background: #0f2a3d; color: #ffffff; padding: 24px;'>
            <h2 style='margin: 0; font-size: 20px;'>Gracias por ponerte en contacto</h2>
            <p style='margin: 8px 0 0 0; font-size: 14px;'>{HtmlEncode(senderName)}</p>
        </div>
        <div style='padding: 24px;'>
            <p style='margin: 0 0 12px 0;'>Hola {name},</p>
            <p style='margin: 0 0 12px 0; line-height: 1.6;'>
                Hemos recibido tu mensaje y nuestro equipo te responderá a la brevedad.
            </p>
            <p style='margin: 0 0 8px 0;'><strong>Resumen de tu mensaje:</strong></p>
            <p style='margin: 0 0 12px 0;'><strong>Asunto:</strong> {subject}</p>
            <div style='background: #f3f4f6; padding: 16px; border-radius: 6px; color: #1f2937; line-height: 1.5;'>
                {message}
            </div>
            <p style='margin: 16px 0 0 0; font-size: 12px; color: #6b7280;'>
                Si necesitas agregar más información, responde a este correo.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private static string HtmlEncode(string value)
        => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string HtmlEncodeWithLineBreaks(string value)
    {
        var encoded = HtmlEncode(value);
        return encoded.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
    }
}
